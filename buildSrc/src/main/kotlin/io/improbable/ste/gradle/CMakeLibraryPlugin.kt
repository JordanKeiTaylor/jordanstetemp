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


import org.gradle.api.Plugin
import org.gradle.api.Project

/**
 * A sample plugin that wraps a CMake build with Gradle to take care of dependency management.
 */
class CMakeLibraryPlugin : Plugin<Project> {
    override fun apply(project: Project) {
        project.pluginManager.apply(WrappedNativeLibraryPlugin::class.java)
        // Add a CMake extension to the Gradle model
        val extension = project.extensions.create("cmake", CMakeExtension::class.java, project.layout, project.objects)

        /*
         * Create some tasks to drive the CMake build
         */
        val tasks = project.tasks

        val cmakeDebug = tasks.create("cmakeDebug", CMake::class.java) { task ->
            task.buildType = "Debug"
            task.includeDirs.from(project.configurations.getByName("cppCompile"))
            task.linkFiles.from(project.configurations.getByName("cppLinkDebug"))
            task.variantDirectory.set(project.file("${project.buildDir}/debug"))
            task.projectDirectory = extension.projectDirectory
            task.args = extension.args
            task.env = extension.env
        }

        val cmakeRelease = tasks.create("cmakeRelease", CMake::class.java) { task ->
            task.buildType = "RelWithDebInfo"
            task.includeDirs.from(project.configurations.getByName("cppCompile"))
            task.linkFiles.from(project.configurations.getByName("cppLinkDebug"))
            task.variantDirectory.set(project.file("${project.buildDir}/release"))
            task.projectDirectory.set(extension.projectDirectory)
            task.args = extension.args
            task.env = extension.env
        }

        val assembleDebug = tasks.create("assembleDebug", Make::class.java) { task ->
            task.group = "Build"
            task.description = "Builds the debug binaries"
            task.generatedBy(cmakeDebug)
            task.binary(extension.binary)
        }

        val assembleRelease = tasks.create("assembleRelease", Make::class.java) { task ->
            task.group = "Build"
            task.description = "Builds the release binaries"
            task.generatedBy(cmakeRelease)
            task.binary(extension.binary)
        }

        tasks.getByName("assemble").dependsOn("assembleDebug")

        /*
         * Configure the artifacts which should be exposed by this build
         * to other Gradle projects. (Note that this build does not currently
         * expose any runtime (shared library) artifacts)
         */
        val configurations = project.configurations
        configurations.getByName("headers").outgoing.artifact(extension.includeDirectory)
        configurations.getByName("linkDebug").outgoing.artifact(assembleDebug.binary)
        configurations.getByName("linkRelease").outgoing.artifact(assembleRelease.binary)
    }
}