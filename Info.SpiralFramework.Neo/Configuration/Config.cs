using SpiralNeo.Configuration.Implementation;
using System.ComponentModel;

namespace SpiralNeo.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
    User Properties:
        - Please put all of your configurable properties here.

    By default, configuration saves as "Config.json" in mod user config folder.    
    Need more config files/classes? See Configuration.cs

    Available Attributes:
    - Category
    - DisplayName
    - Description
    - DefaultValue

    // Technically Supported but not Useful
    - Browsable
    - Localizable

    The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
*/


        [Category("I/O Module")]
        [DisplayName("Trace Log Enabled")]
        [Description("Enables the TRACE log level from Abstraction Games")]
        [DefaultValue(false)]
        public bool TraceLogEnabled { get; set; } = false;

        [Category("I/O Module")]
        [DisplayName("Path Replacement Enabled")]
        [Description("Allow Spiral to replace game paths. Required for external mods to be loaded!")]
        [DefaultValue(true)]
        public bool PathReplacementEnabled { get; set; } = true;

        [Category("I/O Module")]
        [DisplayName("Development Replacement Path")]
        [Description("Development folder to load any replacements from, if present. Leave empty to disable")]
        [DefaultValue(null)]
        public string? DevelopmentReplacementPath { get; set; } = null;

        [Category("I/O Module")]
        [DisplayName("Spiral Splash Enabled")]
        [Description("Adds a Spiral overlay to the Abstraction Games splash screen")]
        [DefaultValue(true)]
        public bool SpiralSplashEnabled { get; set; } = true;
    }
}