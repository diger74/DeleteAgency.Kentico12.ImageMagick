using System;
using System.Collections.Generic;
using CMS;
using CMS.DataEngine;
using CMS.Modules;
using CMSApp.CMSModules;
using NuGet;

[assembly: RegisterModule(typeof(ImageMagickModuleGeneration))]
namespace CMSApp.CMSModules
{
    public class ImageMagickModuleGeneration : Module
    {
        public ImageMagickModuleGeneration() : base("ImageMagickModuleGeneration", false)
        {
            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifestOnAfter;
        }

        private void BuildNuSpecManifestOnAfter(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (!e.ResourceName.Equals("DeleteAgency.Kentico12.ImageMagick", StringComparison.OrdinalIgnoreCase)) return;

            e.Manifest.Metadata.DependencySets = new List<ManifestDependencySet>
            {
                new ManifestDependencySet
                {
                    Dependencies = new List<ManifestDependency>
                    {
                        new ManifestDependency
                        {
                            Id = "Magick.NET-Q16-AnyCPU", Version = "7.14.5"
                        }
                    }
                }
            };
            e.Manifest.Metadata.Owners = "DeleteAgency";
            e.Manifest.Metadata.Authors = "Dmitry Bastron";
            e.Manifest.Metadata.Tags = "Kentico, ImageMagick, image optimization";
        }
    }
}