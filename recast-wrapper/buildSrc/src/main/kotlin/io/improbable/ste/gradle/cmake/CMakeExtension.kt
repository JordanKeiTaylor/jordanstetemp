package io.improbable.ste.gradle.cmake

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

import org.gradle.api.file.ProjectLayout
import org.gradle.api.model.ObjectFactory
import org.gradle.api.provider.Property
import javax.inject.Inject

open class CMakeExtension @Inject constructor(projectLayout: ProjectLayout, objectFactory: ObjectFactory) {
    val binary = objectFactory.property(String::class.java)
    val includeDirectory = projectLayout.directoryProperty()
    val projectDirectory = projectLayout.directoryProperty()

    var args = objectFactory.listProperty(String::class.java)
    var env: Property<Map<*, *>> = objectFactory.property(Map::class.java)

    init {
        projectDirectory.set(projectLayout.projectDirectory)
        includeDirectory.set(projectDirectory.dir("include"))
    }
}