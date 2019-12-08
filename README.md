[![Nuget](https://img.shields.io/badge/nuget-v1.1.0-blue.svg)](https://www.nuget.org/packages/DeleteAgency.Kentico12.ImageMagick/)

# DeleteAgency.Kentico12.ImageMagick
Automatically shrink images using ImageMagick library during uploading to a Media Library or as a Page Attachment. Applicable for Kentico 12 CMS.

## Installation
Install **DeleteAgency.Kentico12.ImageMagick** NuGet package for **both** Kentico CMS and MVC solutions:
```
Install-Package DeleteAgency.Kentico12.ImageMagick
```
Build and run Kentico CMS Admin. During first open Kentico module with settings will be imported in the database automatically.

## Configuration
The following settings can be changed in **Settings > Content > Media > Optimization > ImageMagick** section:
![ImageMagick module settings](/Assets/imagemagick_module_settings.png)

## Contribution
All contributions are very much welcomed! Setting up a development environment is pretty easy:

0. Download the code from the repository:
   ```
   git clone https://github.com/diger74/DeleteAgency.Kentico12.ImageMagick
   ```
1. Install Kentico default **Dancing Goat MVC** sample site
2. Import **ImageMagick_Kentico12_Module.zip** package via Kentico Import interface in Sites application
3. Copy the content of the **CMS** folder from code repository to the Dancing Goat website **CMS** folder
4. Add **DeleteAgency.Kentico12.ImageMagick.csproj** project into Dancing Goat **WebApp** solution, build and open Kentico admin interface

## Compatibility
This NuGet package is compatible with Kentico 12, any Hotfix, both Portal Engine and MVC.

## Support
Found a bug? Please raise an Issue in the GitHub or contact me.

## Referencies
For more information please read my [blog post](https://diger74.net/image-optimization-with-imagemagick).
