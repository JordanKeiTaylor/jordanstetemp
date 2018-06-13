package io.improbable.ste.gradle

import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileCollection
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.*
import org.gradle.api.tasks.util.PatternFilterable

// Adapted from: https://github.com/gradle/native-samples/blob/c40ee8cf25984e8af7642fa6c681a11a2328faed/samples-dev/src/templates/build-wrapper-plugin/src/main/groovy/org/gradle/samples/tasks/CMake.groovy
open class CMake : DefaultTask() {
    @get:Input
    var buildType: String? = null

    @get:Internal
    var variantDirectory: DirectoryProperty = newOutputDirectory()

    @get:Internal
    var projectDirectory: DirectoryProperty = newInputDirectory()

    @get:InputFiles
    val includeDirs: ConfigurableFileCollection = project.files()

    @get:InputFiles
    val linkFiles: ConfigurableFileCollection = project.files()

    @get:Internal
    lateinit var args: ListProperty<String>

    @get:Internal
    lateinit var env: Property<Map<*, *>>

    @TaskAction
    fun generateCmakeFiles() {
        val cmakeExecutable = System.getenv("CMAKE_EXECUTABLE") ?: "cmake"

        variantDirectory.get().asFile.mkdirs()
        val commandLine = mutableListOf<String>(cmakeExecutable,
            "-DCMAKE_BUILD_TYPE=${buildType!!.capitalize()}",
            "-DINCLUDE_DIRS=${includeDirs.joinToString("; ")}",
            "-DLINK_DIRS=${linkFiles.map{ it.parent }.joinToString(";")}",
            "--no-warn-unused-cli")

        commandLine.addAll(args.get())
        commandLine.add(projectDirectory.get().asFile.absolutePath)

        project.exec { exec ->
            exec.workingDir(variantDirectory.get())
            exec.commandLine(commandLine)

            env.get().entries.forEach { exec.environment[it.key.toString()] = it.value.toString() }
        }
    }

    @InputFiles
    fun getCMakeLists(): PatternFilterable? {
        return project.fileTree(projectDirectory.get().asFile).include("**/CMakeLists.txt")
    }

    @OutputFiles
    fun getCmakeFiles(): FileCollection =
        project.fileTree(variantDirectory.get())
                .include("**/CMakeFiles/**/*")
                .include("**/Makefile")
                .include("**/*.cmake")
                as FileCollection
}