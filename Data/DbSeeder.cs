using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Agencies.AnyAsync()) return;

            // 1. Seed Agencies
            var agencies = new List<Agency>
            {
                new Agency { Abbreviation = "ACI", FullName = "American Concrete Institute" },
                new Agency { Abbreviation = "MARTCP", FullName = "Mid-Atlantic Region Technician Certification Program" },
                new Agency { Abbreviation = "ICC", FullName = "International Code Council" },
                new Agency { Abbreviation = "WACEL", FullName = "Washington Area Council of Engineering Laboratories" },
                new Agency { Abbreviation = "NICET", FullName = "National Institute for Certification in Engineering Technologies" },
                new Agency { Abbreviation = "NRMCA", FullName = "National Ready Mixed Concrete Association" },
                new Agency { Abbreviation = "VDOT", FullName = "Virginia Department of Transportation" },
                new Agency { Abbreviation = "OSHA", FullName = "Occupational Safety and Health Administration" },
                new Agency { Abbreviation = "WVDOH", FullName = "West Virginia Division of Highways" },
                new Agency { Abbreviation = "AWS", FullName = "American Welding Society" },
                new Agency { Abbreviation = "Others", FullName = "Other Agencies" }
            };

            context.Agencies.AddRange(agencies);
            await context.SaveChangesAsync();

            // Refetch agencies to get valid Ids
            var agencyMap = await context.Agencies.ToDictionaryAsync(a => a.Abbreviation, a => a.Id);

            // 2. Seed Certifications
            var rawCertNames = new List<string>
            {
                "ACI Concrete Transportation Construction Inspector In Training",
                "ACI Concrete Transportation Construction Inspector",
                "ACI Aggregate Testing Technician Level I",
                "ACI Aggregate Testing Technician Level I",
                "ACI Aggregate/Sols Base Testing Technician",
                "ACI Cement Physical Tester",
                "ACI Concrete Field Technician Grade I",
                "ACI Concrete Field Technician Level II",
                "ACI Concrete Laboratory Testing Technician Level I",
                "ACI Concrete Laboratory Testing Technician Level I",
                "ACI Concrete Strength Testing Technician",
                "ACI Masonry Field Testing Technician",
                "ACI Masonry Laboratory Testing Technician",
                "Aerosol 3-Day Asbestos Inspector",
                "Asbestos Awareness",
                "Asbestos Inspector",
                "Asphalt Institute Asphalt Binder Technology",
                "Asphalt Institute Asphalt Mix Design Technology",
                "Asphalt Institute Optimizing Volumetrics and HMA Compactability (Bailey Method)",
                "AWS Certified Welding Inspector",
                "AWS Welding Supervisor",
                "CETCO Bentonite Waterproofing System",
                "CETCO Certified Waterproofing Inspector",
                "ClickSafety Asbestos in Construction",
                "Coatings Application Specialist",
                "Confined Space Awareness",
                "Cont-Confined Spaces Awareness",
                "Cont-Heat Stress Orientation",
                "Cont-Respiratory Protection Training",
                "DC Risk Assessor/Lead",
                "DOT, NRC & IATA Requirements for Shipping Radioactive Material",
                "DOT, NRC & IATA Requirements for Shipping Radioactive Material with Radiation Fundamentals",
                "Dye Penetrant Testing, Level II",
                "Erosion and Sediment Control Designers Training",
                "FTI Aerial/Scissor Lift Training",
                "HAZMAT Certification",
                "ICC Soils Special Inspector",
                "Magnetic Particle Testing Level I",
                "MARTCP Aggregate Technician (Quarry/Producers Laboratory)",
                "MARTCP Concrete Field",
                "MARTCP HMA Field Technician",
                "MARTCP HMA Materials Tester",
                "MARTCP Plant Technician Level I",
                "MARTCP Plant Technician Level II",
                "Maryland Asbestos License",
                "Maryland Department of Environment (MDE) Erosion & Sediment Control (Green Card)",
                "Maryland Lead Risk Assessor",
                "Maryland MDE Lead Paint Inspector Technician",
                "Maryland Water Sampler Certificate",
                "Maryland MDOT SHA Temporary Traffic Control Manager (TTCM)",
                "MDOT SHA Erosion & Sediment Control (Yellow Card)",
                "NACE Cured In Place Inspector",
                "NICET Asphalt Level I",
                "NICET Asphalt Level II",
                "NICET Asphalt Level III",
                "NICET Asphalt Level IV",
                "NICET Concrete Level I",
                "NICET Concrete Level II",
                "NICET Concrete Level III",
                "NICET Concrete Level IV",
                "NICET Highway Construction Level I",
                "NICET Highway Construction Level II",
                "NICET Highway Materials Level I",
                "NICET Highway Materials Level II",
                "NICET Soils Level I",
                "NICET Soils Level II",
                "NICET Soils Level III",
                "NICET Soils Level IV",
                "NRCA Advanced Roofing Technology: The Master’s Course",
                "NRCA Performance Technology for Roof Systems",
                "NRMMC Concrete Field Testing Technician Grade II",
                "NRMC Pervious Concrete Craftsman",
                "NRMC Pervious Concrete Technician",
                "OSHA 10 Hour",
                "OSHA 26, Course 510",
                "OSHA 30-hour Construction Safety & Health",
                "OSHA Permit-Required Confined Space Entry",
                "Pennsylvania Asbestos Lead Inspector",
                "PA Risk Assessor/Lead",
                "Panametrix - NDT Ultrasonic NDT Level I",
                "Panametrix - NDT Ultrasonic NDT Level II",
                "PCI Quality Control Personnel Certification Level III",
                "Permit Confined Spaces",
                "Punyam: CertCalibration Lab Engineer Mech",
                "SSPC C-2 Supervisor/Competent Deleading of Industrial Structures",
                "SSPC C-3 Supervisor/Competent Deleading of Industrial Structures",
                "Stork Welder Qualification Record",
                "Troxler Compaction & Density Testing",
                "Troxler HAZMAT Certification",
                "Troxler Radiation Safety Officer Training",
                "Troxler Soil & Aggregate Compaction",
                "U.S. DOT National Highway Institute Drilled Shaft Foundation",
                "VDOT Asphalt Field & Compaction Technician",
                "VDOT Asphalt Plant Technician",
                "VDOT Asphalt Plant Level I",
                "VDOT Concrete Field",
                "VDOT Concrete Field Technician",
                "VDOT Concrete Field Testing",
                "VDOT Asphalt Plant Level II",
                "VDOT Asphalt Field Level I",
                "VDOT Soils & Aggregate Compaction",
                "VDOT Surface Treatment",
                "Virginia Lead Inspector License",
                "Visual Testing (VT) Level II",
                "WACEL Aggregate Laboratory Technician",
                "WACEL Concrete Field Technician Level I",
                "WACEL Concrete/Masonry Strength Technician",
                "WACEL Concrete Field Technician Level II",
                "WACEL Structural Concrete",
                "WACEL Firestopping Technician",
                "WACEL Foundation Technician Level I",
                "WACEL Structural Masonry Special Inspector",
                "WACEL Structural Steel Inspector Level I",
                "WACEL Structural Steel Inspector Level II",
                "West Virginia Asbestos Inspector",
                "West Virginia Lead Inspector",
                "WVDOH Aggregate Technician",
                "WVDOH Asphalt Field & Compaction Technician",
                "WVDOH Asphalt Plant Technician",
                "WVDOH Certified Concrete Technologist",
                "WVDOH Portland Cement Concrete Inspector",
                "WVDOH Portland Cement Concrete Technician",
                "WVDOH Soil & Aggregate Compaction Inspector",
                "WVDOH Superpave Mix Design Technician",
                "WVDOH Transportation Engineering Technician (TRET) Level I",
                "WVDOH Transportation Engineering Technician (TRET) Level II",
                "WVDOH Transportation Engineering Technician (TRET) Level III",
                "WVDOH Transportation Engineering Technician (TRET) Level IV",
                "WVDOH Transportation Engineering Technologist (TRETNO) Level V"
            };

            var certifications = rawCertNames.Select(name => new Certification
            {
                Name = name,
                AgencyId = DetermineAgencyId(name, agencyMap),
                ValidityPeriodMonths = 36, // Default validity
                IsActive = true
            }).ToList();

            context.Certifications.AddRange(certifications);
            await context.SaveChangesAsync();
        }

        private static int DetermineAgencyId(string certName, Dictionary<string, int> agencyMap)
        {
            if (certName.StartsWith("ACI")) return agencyMap["ACI"];
            if (certName.StartsWith("MARTCP")) return agencyMap["MARTCP"];
            if (certName.StartsWith("ICC")) return agencyMap["ICC"];
            if (certName.StartsWith("WACEL")) return agencyMap["WACEL"];
            if (certName.StartsWith("NICET")) return agencyMap["NICET"];
            if (certName.StartsWith("NRMCA") || certName.StartsWith("NRMC") || certName.StartsWith("NRMMC")) return agencyMap["NRMCA"];
            if (certName.StartsWith("VDOT")) return agencyMap["VDOT"];
            if (certName.StartsWith("OSHA")) return agencyMap["OSHA"];
            if (certName.StartsWith("WVDOH")) return agencyMap["WVDOH"];
            if (certName.StartsWith("AWS")) return agencyMap["AWS"];
            
            return agencyMap["Others"];
        }
    }
}
