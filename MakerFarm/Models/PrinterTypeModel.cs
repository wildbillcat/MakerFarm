using System;
using System.ComponentModel.DataAnnotations;

namespace MakerFarm.Models
{
    public class PrinterType
    {
        
        public int PrinterTypeId { get; set; } /* Generic Id for the Printer Type/Model */

        [Display(Name = "Printer Type Name", Description = "Name of the Printer Type, such as Ultrabot or Replicator 2")]
        public string TypeName { get; set; } /* The printer model/type name */

        [Range(1, int.MaxValue, ErrorMessage = "Value Should be between 1 and 2,147,483,647")]
        [Display(Name = "Maximum Number of Supported Materials", Description = "This is the maximum number of materials ")]
        public int SupportedNumberMaterials { get; set; } /* The number of materials supported for sumultanious use. (IE, how many extrution heads available on the printer) */

        [Display(Name = "Printer Unit of Measure", Description = "This defines what unit of measure it used for the printer. (Gram, Ounce, Cubic Inch)")]
        public string MaterialUseUnit { get; set; } /* Printers use a varying units of measure, this allows the operator to make sure the appropriate chage is made */
                
        [Display(Name = "Printer Icon", Description = "This should be a image to double at the printer Icon when users select what printer they would like to submit to")]
        public string PrinterIcon { get; set; } /*This is the icon that was uploaded to represent the printer. */

        [Range(1, int.MaxValue, ErrorMessage = "Value Should be between 1 and 2,147,483,647")]
        [Display(Name = "Maximum Authorized Attempts", Description = "This is the Maximum number of attempts a user can authorize on this printer")]
        public int MaxNumberUserAttempts { get; set; } /* This notes the maximum number of attempts allowed for this printer */

        [Display(Name = "Supported File Types", Description = "A comma separated list of file types accepted by this printer. Ie: gcode,x3g,s3g,etc")]
        public string SupportedFileTypes { get; set; } /* This is a comma separated list of the printers file extensions */

        [Display(Name = "Print Submission Comment Field", Description = "This text will be prepopulated into the Print Submission Form Comment Field, and should give printer specific instructions for the user")]
        public string CommentField { set; get; }

        [Display(Name = "About the Printer", Description = "This text will be Displayed at the top of the queue page to inform users about the type of printer they are looking at.")]
        public string AboutPrinter { set; get; }

        [Display(Name = "Printer Information URL", Description = "This URL will be placed underneath the about the printer section of the Printer Page.")]
        public string HyperLink { set; get; }

        [Range(1, int.MaxValue, ErrorMessage = "Value Should be between 1 and 2,147,483,647")]
        [Display(Name = "Maximum number of Copies", Description = "This determines the Maximum number of copies a user can request of a single submission")]
        public int MaximumNumberOfCopies { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Maximum length of the build volume (Inch)", Description = "This determines the Maximum length of the build volume for the printer")]
        public double BuildLength { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Maximum width of the build volume (Inch)", Description = "This determines the Maximum width of the build volume for the printer")]
        public double BuildWidth { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Maximum height of the build volume (Inch)", Description = "This determines the Maximum height of the build volume for the printer")]
        public double BuildHeight { set; get; }

        [Display(Name = "Offers breakaway support capability?", Description = "This determines if the printer can use some sort of support in order to build parts that are not self supporting")]
        public bool OffersBreakawaySupport { set; get; }
        
        [Display(Name = "Offers non-breakaway support capability?", Description = "This determines if the printer can use some sort of support such as disolvable support, or builds using a method which nullifys a support material requirment")]
        public bool OffersNonBreakAwaySupport { set; get; }

        [Display(Name = "Build non-breakaway support Uses MaterialSlot?", Description = "This determines if the printer uses a separate material to build support structure ie. soluble support, thus reducing the number of available materials")]
        public bool BuildSupportUsesMaterialSlot { set; get; }

        [Display(Name = "Offers full color printing?", Description = "This determines if the printer can use a single material and print in full color (ie with pigments or dyes)")]
        public bool OffersFullColorPrinting { set; get; }

        [Display(Name = "Functional model support?", Description = "This denotes if the printer uses a material solid enough to make functional parts, such as plastic printers.")]
        public bool FunctionalModelSupport { set; get; }

        [Display(Name = "Allow users to see queue?", Description = "This denotes if the printer queue should be avaiable to see for users.")]
        public bool QueueVisible { set; get; }

        [Display(Name = "Allow users to submit to queue?", Description = "This denotes if the printer queue should be avaiable for submission for users.")]
        public bool SubmissionEnabled { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Cost of print per unit for internal users", Description = "This is the cost per unit that should be charged for internal users")]
        public double InternalCostPerUnit { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Cost of print per unit for external users", Description = "This is the cost per unit that should be charged for external users")]
        public double ExternalCostPerUnit { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Initial Cost for internal users", Description = "This is the cost that should be charged to internal users for each print submitted, in addition to the Material Cost")]
        public double InternalInitialCost { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        [Display(Name = "Initial cost for external users", Description = "This is the cost that should be charged to external users for each print submitted, in addition to the Material Cost")]
        public double ExternalInitialCost { set; get; }

        public bool EnhancedGcodeViewerEnabled {set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        public double GCodePlasticDiameter { set; get; }

        public PlasticTypes PlasticType { set; get; }

        [Range(0, double.MaxValue, ErrorMessage = "Value Should be between 0 and 2.46552022185745164e+303 Nautical Miles (Imagine what that is in inches!)")]
        public double NozzleSize { get; set; }
    }

    public enum PlasticTypes
    {
        PLA, ABS
    }
}