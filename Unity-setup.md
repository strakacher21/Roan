# Unity project setup

Follow these steps to set up the Unity project:

1. **Download or Clone the Project**  
   Download the project as a [ZIP file](https://github.com/strakacher21/Roan/archive/refs/heads/main.zip) or clone it using Git:  
   ```
   https://github.com/strakacher21/Roan.git
   ```

2. **Install VRChat Creator Companion (VCC)**  
   Download the latest version of the [VRChat Creator Companion (VCC)](https://vrchat.com/download/vcc) and follow the [official installation guide](https://vcc.docs.vrchat.com/) to set it up.

3. **Add Custom Packages to the repository**  
   This project uses custom packages from [hai-vr](https://github.com/hai-vr) and [PoiyomiToonShader](https://github.com/poiyomi/PoiyomiToonShader). In VCC, navigate to Settings > Packages > Add Repository, then paste the following URLs:  
   
   ```
   https://hai-vr.github.io/vpm-listing/index.json
   ```

   ```
   https://poiyomi.github.io/vpm/index.json
   ```

4. **Add the Unity Project to VCC**  
   In **VCC**, add the `Roan unity project` folder to your project list by selecting **Add Existing Project** and navigating to the project directory.

5. **Manage the Project**  
   Select the project in VCC and click **Manage Project**. This will open a dialog window.

6. **Resolve Dependencies**  
   In the dialog window, click **Resolve** to automatically install all required packages and dependencies for the project.

7. **Open the Project**  
   Once dependencies are resolved, click **Open Project** to launch the project in Unity (ensure you are using **Unity 2022.3.22f1**).