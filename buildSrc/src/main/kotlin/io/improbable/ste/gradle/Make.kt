package io.improbable.ste.gradle

/*
 * Copyright 2018 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileCollection
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.TaskAction
import org.gradle.api.provider.Provider
import org.gradle.api.provider.ListProperty

open class Make : DefaultTask() {
    @get:Internal
    var variantDirectory: DirectoryProperty = newInputDirectory()

    @get:InputFiles
    lateinit var makeFiles: FileCollection

    @get:OutputDirectory
    var outputDirectory: DirectoryProperty = newInputDirectory()

    var binary: RegularFileProperty? = newOutputFile()
    var arguments: ListProperty<String> = getProject().getObjects().listProperty(String::class.java)

    @TaskAction
    fun executeMake() {
        val makeExecutable = System.getenv("MAKE_EXECUTABLE") ?: "make"
        project.exec { exec ->
            exec.workingDir(variantDirectory)

            val allArguments = mutableListOf(makeExecutable)
            allArguments.addAll(arguments.get())
            exec.commandLine(allArguments)
        }
    }

    fun generatedBy(cmake: CMake) {
        variantDirectory.set(cmake.variantDirectory)
        outputDirectory.set(cmake.variantDirectory)
        makeFiles = cmake.getCmakeFiles()
    }

    fun binary(path: Provider<String>) {
        binary!!.set(outputDirectory.file(path))
    }
}