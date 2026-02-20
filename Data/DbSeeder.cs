using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Models;

namespace SeHrCertificationPortal.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. Seed Agencies & Certifications
            if (!await context.Agencies.AnyAsync())
            {

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

            // 3. Seed Enterprise Mock Data (35 Employees, ~750 Requests)
            if (!await context.Employees.AnyAsync())
            {
                var random = new Random();

                // --- Generate Employees ---
                var firstNames = new[] { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "David", "Elizabeth", "William", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
                var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };

                var employees = new List<Employee>();
                for (int i = 0; i < 35; i++)
                {
                    var first = firstNames[random.Next(firstNames.Length)];
                    var last = lastNames[random.Next(lastNames.Length)];
                    // Ensure unique emails roughly
                    var email = $"{first.ToLower()}.{last.ToLower()}{random.Next(10,99)}@specializedengineering.com";
                    
                    employees.Add(new Employee 
                    { 
                        DisplayName = $"{first} {last}"
                    });
                }
                context.Employees.AddRange(employees);
                await context.SaveChangesAsync(); // IDs generated

                // --- Fetch Catalog for Reference ---
                var agencyIds = await context.Agencies.Select(a => a.Id).ToListAsync();
                var certIds = await context.Certifications.Select(c => c.Id).ToListAsync();
                var employeeIds = await context.Employees.Select(e => e.Id).ToListAsync();

                var managers = new[] { "Sarah Connor", "Kyle Reese", "John Doe", "Ellen Ripley", "Carter Burke" };
                var requests = new List<CertificationRequest>();

                // --- Generate 750 Certification Requests ---
                for (int i = 0; i < 750; i++)
                {
                    // Random Date (Last 10 years)
                    var daysBack = random.Next(0, 3650);
                    var requestDate = DateTime.UtcNow.AddDays(-daysBack);

                    // Employee & Manager
                    var empId = employeeIds[random.Next(employeeIds.Count)];
                    var manager = managers[random.Next(managers.Length)];

                    // Request Data
                    int? agencyId = null;
                    // int? certId = null; // Removed unused variable
                    string? customAgency = null;
                    string? customCert = null;

                    // 10% Custom, 90% Catalog
                    if (random.NextDouble() < 0.10) 
                    {
                        var customAgencies = new[] { "Maryland Dept of Environment", "Hagerstown Community College", "FEMA", "Red Cross" };
                        var customCerts = new[] { "Advanced Safety", "Water Quality", "First Aid", "Drone Pilot" };
                        customAgency = customAgencies[random.Next(customAgencies.Length)];
                        customCert = customCerts[random.Next(customCerts.Length)];
                    }
                    else
                    {
                        if (agencyIds.Any()) agencyId = agencyIds[random.Next(agencyIds.Count)];
                        // Ideally pick a cert that matches the agency, but for stress testing random is okay 
                        // or we can fetch the map. Let's filter certs by agency to be realistic if possible.
                        // For speed in seeder, we'll just pick a random cert, and assume the agency link 
                        // in the certification table is the source of truth, but here we set both.
                        // Let's improve: Pick a random CertID, then look up its AgencyID? 
                        // That requires loading the whole map. 
                        // Simplified approach for speed as per prompt "randomly pick a valid AgencyId and CertificationId":
                        // To be "valid", let's load Cert objects with AgencyId.
                    }

                    // Re-fetching Certs with AgencyId to ensure consistency
                    // (Done efficiently outside loop below)
                }
                
                // Optimized Loop with correct relational data
                var certsWithAgency = await context.Certifications.Select(c => new { c.Id, c.AgencyId }).ToListAsync();

                requests.Clear(); // Restart loop with better data
                for (int i = 0; i < 750; i++)
                {
                     // Random Date (Last 10 years)
                    var daysBack = random.Next(0, 3650);
                    var requestDate = DateTime.UtcNow.AddDays(-daysBack);

                     // Status Logic
                    RequestStatus status;
                    bool isOlderThan60Days = daysBack > 60;
                    if (isOlderThan60Days)
                    {
                        // 90% Approved, 10% Rejected
                        status = random.NextDouble() < 0.90 ? RequestStatus.Approved : RequestStatus.Rejected;
                    }
                    else
                    {
                        // Recent: Mix of Pending, Approved, Rejected
                        var roll = random.Next(3);
                        status = roll == 0 ? RequestStatus.Pending : (roll == 1 ? RequestStatus.Approved : RequestStatus.Rejected);
                    }

                    // Request Type
                    var requestType = (RequestType)random.Next(0, 6); // Enum has ~6 values

                    var empId = employeeIds[random.Next(employeeIds.Count)];
                    var manager = managers[random.Next(managers.Length)];

                    var req = new CertificationRequest
                    {
                        EmployeeId = empId,
                        ManagerName = manager,
                        RequestDate = requestDate,
                        Status = status,
                        RequestType = requestType
                    };

                    // 10% Custom
                    if (random.NextDouble() < 0.10)
                    {
                        req.CustomAgencyName = "External Training Provider"; 
                        req.CustomCertificationName = "Specialized Workshop";
                         // Add varitey
                        var customAgencies = new[] { "Maryland Dept of Environment", "Hagerstown Community College", "FEMA", "Red Cross" };
                        req.CustomAgencyName = customAgencies[random.Next(customAgencies.Length)];
                    }
                    else if (certsWithAgency.Any())
                    {
                        // Pick random valid cert
                        var certData = certsWithAgency[random.Next(certsWithAgency.Count)];
                        req.CertificationId = certData.Id;
                        req.AgencyId = certData.AgencyId;
                    }

                    requests.Add(req);
                }

                context.CertificationRequests.AddRange(requests);
                await context.SaveChangesAsync();
            }
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
