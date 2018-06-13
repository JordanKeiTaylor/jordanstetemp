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
import org.gradle.api.attributes.Usage
import org.gradle.language.cpp.CppBinary

class WrappedNativeLibraryPlugin : Plugin<Project> {
    @Override
    override fun apply(project: Project) {
        project.pluginManager.apply("lifecycle-base")

        /*
         * Define some configurations to present the outputs of this build
         * to other Gradle projects.
         */
        val cppApiUsage = project.objects.named(Usage::class.java, Usage.C_PLUS_PLUS_API)
        val linkUsage = project.objects.named(Usage::class.java, Usage.NATIVE_LINK)
        val runtimeUsage = project.objects.named(Usage::class.java, Usage.NATIVE_RUNTIME)

        // dependencies of the library
        val implementation = project.configurations.create("implementation") {
            it.isCanBeConsumed = false
            it.isCanBeResolved = false
        }

        // incoming compile time headers - this represents the headers we consume
        project.configurations.create("cppCompile") {
            it.isCanBeConsumed = false
            it.extendsFrom(implementation)
            it.attributes.attribute(Usage.USAGE_ATTRIBUTE, cppApiUsage)
        }

        // incoming linktime libraries (i.e. static libraries) - this represents the libraries we consume
        project.configurations.create("cppLinkDebug") {
            it.isCanBeConsumed = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, linkUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, false)
            }
        }

        project.configurations.create("cppLinkRelease") {
            it.isCanBeConsumed = false
            it.extendsFrom(implementation)
            it.attributes {
                it. attribute(Usage.USAGE_ATTRIBUTE, linkUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, true)
            }
        }

        // incoming runtime libraries (i.e. shared libraries) - this represents the libraries we consume
        project.configurations.create("cppRuntimeDebug") {
            it.isCanBeConsumed = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, runtimeUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, false)
            }
        }

        project.configurations.create("cppRuntimeRelease") {
            it.isCanBeConsumed = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, runtimeUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, true)
            }
        }

        // outgoing public headers - this represents the headers we expose (including transitive headers)
        project.configurations.create("headers") {
            it.isCanBeResolved = false
            it.extendsFrom(implementation)
            it.attributes.attribute(Usage.USAGE_ATTRIBUTE, cppApiUsage)
        }

        // outgoing linktime libraries (i.e. static libraries) - this represents the libraries we expose (including transitive headers)
        project.configurations.create("linkDebug") {
            it.isCanBeResolved = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, linkUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, false)
            }
        }

        project.configurations.create("linkRelease") {
            it.isCanBeResolved = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, linkUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, true)
            }
        }

        // outgoing runtime libraries (i.e. shared libraries) - this represents the libraries we expose (including transitive headers)
        project.configurations.create("runtimeDebug") {
            it.isCanBeResolved = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, runtimeUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, false)
            }
        }

        project.configurations.create("runtimeRelease") {
            it.isCanBeResolved = false
            it.extendsFrom(implementation)
            it.attributes {
                it.attribute(Usage.USAGE_ATTRIBUTE, runtimeUsage)
                it.attribute(CppBinary.DEBUGGABLE_ATTRIBUTE, true)
                it.attribute(CppBinary.OPTIMIZED_ATTRIBUTE, true)
            }
        }
    }
}