# Controller

### Description

This is a free, open source, third person controller with built-in features such as locomotion, inventory, user interface, weapons, save/load system and more. The goal of this project is to create a well rounded, high quality, complete third person controller that can be easily adjusted into multiple sub-genres. 

Developed and tested on Unity Versions **`2020.3+`**.

### Installation

- **First** you'll have to navigate to `Windows > Package Manager` and instal [GitDependencyResolver](https://github.com/mob-sakai/GitDependencyResolverForUnity#installation)

- **Then** you can add the Player Package using the link https://github.com/rob1997/Controller.git?path=/Packages/com.ekaka.player#main following the steps [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

If you run into issues while resolving dependencies take a look under the [usage](https://github.com/mob-sakai/GitDependencyResolverForUnity#usage) section of GitDependencyResolver for possible solutions.

### Import Sample

- Before importing samples navigate to `Windows > Asset Management > Addressables > Groups` which will open a new window where you can `Create Addressable Settings`.

- Once you've created the addressable settings navigate back to `Windows > Package Manager:In Project` and select the Player package and Import the `Universal RP Sample Demo` under `Samples`

To run the sample simply open `0_Loading` scene at `Assets/Samples/Player/{version}/Universal RP Demo/Scenes/` and press Play. You can also take a look at the [SampleProject](https://github.com/rob1997/Controller/tree/main/SampleProject) which has the sample already imported and setup.

### Controls

Currently only PC (Keyboard and Mouse) controls have been assigned.

+ `WSAD : Move`
+ `Mouse : Look`
+ `E : Pick/Use`
+ `Mouse Scroll : Change Item`

### Previews

| **Weapons and Inventory** | **Locomotion** |
| ------------------------- | -------------- |
| ![image](https://rob1997.github.io/Images/Controller_1.gif) | ![image](https://rob1997.github.io/Images/Grounder.gif) |
