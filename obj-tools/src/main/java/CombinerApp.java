import org.apache.commons.lang3.StringUtils;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.Writer;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Objects;
import java.util.Set;
import java.util.TreeSet;
import java.util.stream.Collectors;

public class CombinerApp {
    private final static boolean USE_TEXTURES = true;
    private final static boolean USE_GLOBAL_V = true;

    private static Map<String, Long> vRoundedMap = new HashMap<>();

    private static Map<String, Long> vMap = new HashMap<>();
    private static Long vOffset = 0L;
    private static Long vtOffset = 0L;
    private static Long duplicateV = 0L;
    private static Long ignoredFaces = 0L;

    private static Set<File> everyObjFile(String dir) {
        Set<File> returnThis = new TreeSet<>();

        File dirFile = new File(dir);

        if (!dirFile.isDirectory()) {
            System.err.println("Not a directory [" + dir + "]");
            return returnThis;
        }

        for (File file : Objects.requireNonNull(dirFile.listFiles())) {
            if (file.isFile() && file.getName().endsWith(".obj")) {
                returnThis.add(file);
            }
        }

        return returnThis;
    }

    private static void addObjFile(File objFile, Writer writer) {
        System.out.println("Adding: " + objFile.getAbsolutePath());
        try (BufferedReader bf = new BufferedReader(new FileReader(objFile))) {
            Map<Long, Long> localToGlobalVMap = new HashMap<>();
            Long[] localVOffset = { 1L };
            Long[] vSize = { 0L };
            Long[] vtSize = { 0L };

            for (String line : bf.lines().collect(Collectors.toList())) {
                processLine(writer, localToGlobalVMap, localVOffset, vSize, vtSize, line);
            }
            vOffset += vSize[0];
            vtOffset += vtSize[0];
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private static String rescale(String numEx) {
        // -1.04584230e+03 -> -1045.840
        //  3.61781064e+02 ->  361.781
        return (new BigDecimal(numEx)).setScale(3, RoundingMode.DOWN).toPlainString();
    }

    private static void trackCloseEnough(String line) {
        String[] v = line.split(" ");
        v[1] = rescale(v[1]);
        v[2] = rescale(v[2]);
        v[3] = rescale(v[3]);
        String vRounded = StringUtils.join(v, " ");
        vRoundedMap.put(vRounded, vRoundedMap.getOrDefault(vRounded, 0L) + 1L);
    }

    private static Long countCloseEnough() {
        return  vRoundedMap
                .values()
                .stream()
                .reduce(0L, (a, b) -> a + b)
                - vRoundedMap.size();
    }

    private static void processLine(Writer writer,
                                    Map<Long, Long> localToGlobalVMap,
                                    Long[] localVOffset,
                                    Long[] vSize,
                                    Long[] vtSize,
                                    String line) throws IOException {
        if (line.startsWith("mtllib ")) {
            if (USE_TEXTURES) {
                writer.write(line);
                writer.write("\n");
            }
        } else if (line.startsWith("usemtl ")) {
            if (USE_TEXTURES) {
                writer.write(line);
                writer.write("\n");
            }
        } else if (line.startsWith("v ")) {
            if (USE_GLOBAL_V) {
                if (vMap.containsKey(line)) {
                    localToGlobalVMap.put(localVOffset[0]++, vMap.get(line));
                    duplicateV++;
                } else {
                    vMap.put(line, (long)(vMap.size() + 1));
                    localToGlobalVMap.put(localVOffset[0]++, (long)vMap.size());
                    writer.write(line);
                    writer.write("\n");
                }
            } else {
                vSize[0]++;
                writer.write(line);
                writer.write("\n");
            }
            trackCloseEnough(line);
        } else if (line.startsWith("vt ")) {
            vtSize[0]++;
            writer.write(line);
            writer.write("\n");
        } else if (line.startsWith("f ")) {
            processFace(writer, localToGlobalVMap, line);
        } else {
            System.err.println("UNKNOWN line: " + line);
        }
    }

    private static void processFace(Writer writer,
                                    Map<Long, Long> localToGlobalVMap,
                                    String line) throws IOException {
        String[] face = line.split(" ");
        if (face.length < 4) {
            System.err.println("Face size bad (" + face.length + "): " + line);
        } else {
            boolean hasUniqueVs = true;
            Set<Long> seen = new HashSet<>();
            // convert local offsets to global offsets
            for (int i = 1; i < face.length; i++) {
                String f = face[i];
                String[] parts = f.split("/");
                if (parts.length != 2) {
                    // not technically required to have a v and vt, but assumed to be normal for our files-- can change in the future
                    System.err.println("Bad face part (" + parts.length + "):" + f);
                } else {
                    Long v = new Long(parts[0]);
                    if (USE_GLOBAL_V) {
                        v = localToGlobalVMap.get(v);
                    } else {
                        v += vOffset;
                    }
                    Long vt = new Long(parts[1]) + vtOffset;
                    if (seen.contains(v)) {
                        hasUniqueVs = false;
                    }
                    seen.add(v);
                    face[i] = v + "/" + vt;
                }
            }

            if (hasUniqueVs) {
                writer.write(StringUtils.join(face, " "));
                writer.write("\n");
            } else {
                ignoredFaces++;
            }
        }
    }

    public static void main(String[] args) {
        String inPath = "/Users/albertlaw/Downloads/Muscat 100m OBJ/Data";
        int level = 22;
        String inDir = inPath + "/L" + level;

        String outPath = inDir + "/L" + level + ".obj";

        try {
            Set<File> objFiles = everyObjFile(inDir);
            FileWriter fw = new FileWriter(outPath, false);
            objFiles
//                    .stream()
//                    .filter( file -> file.getName().contains("007_+008") || file.getName().contains("008_+008"))
                    .forEach( file -> addObjFile(file, fw));
            fw.close();
            System.out.println("Written to: " + outPath);
            System.out.println("Duplicate Vs: " + duplicateV);
            System.out.println("Ignored faces: " + ignoredFaces);
            System.out.println("Close enough: " + countCloseEnough());
        } catch (Throwable t) {
            t.printStackTrace();
        }

        System.out.println("Done!");
    }
}
