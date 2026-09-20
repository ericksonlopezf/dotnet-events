using System;
using System.IO;
using System.Text;

namespace AuditGenerator;

public static partial class Program
{
    public static void Main()
    {
        string currentDir = AppContext.BaseDirectory;
        DirectoryInfo? dir = new DirectoryInfo(currentDir);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EricksonLopez.Events.slnx")))
        {
            dir = dir.Parent;
        }
        string rootDir = dir?.FullName ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        string outDir = Path.Combine(rootDir, "tools", "MEGA-AUDIT");
        Directory.CreateDirectory(outDir);

        Console.WriteLine($"Generating Mega-Audit artifacts in: {outDir}");

        Generate00_ExecutiveSummary(outDir);
        Generate01_SystemInventory(outDir);
        Generate02_ArchitectureAudit(outDir);
        Generate03_EventModelAudit(outDir);
        Generate04_DispatchAudit(outDir);
        Generate05_HandlerLifecycleAudit(outDir);
        Generate06_ConcurrencyAudit(outDir);
        Generate07_ReliabilityAudit(outDir);
        Generate08_TransactionAudit(outDir);
        Generate09_OutboxIntegrationAudit(outDir);
        Generate10_SecurityAudit(outDir);
        Generate11_MultiTenancyAudit(outDir);
        Generate12_ApiDxAudit(outDir);
        Generate13_PerformanceAudit(outDir);
        Generate14_MemoryAudit(outDir);
        Generate15_ResilienceAudit(outDir);
        Generate16_CancellationAudit(outDir);
        Generate17_ShutdownAudit(outDir);
        Generate18_ObservabilityAudit(outDir);
        Generate19_AotTrimmingAudit(outDir);
        Generate20_SerializationAudit(outDir);
        Generate21_FuzzingAudit(outDir);
        Generate22_MutationTestingAudit(outDir);
        Generate23_ChaosTestingAudit(outDir);
        Generate24_CompatibilityAudit(outDir);
        Generate25_DocumentationAudit(outDir);
        Generate26_NugetAudit(outDir);
        Generate27_StaticAnalysis(outDir);
        Generate28_EcosystemIntegrationAudit(outDir);
        Generate29_ApiCompatibility(outDir);
        Generate30_FindingsRegister(outDir);
        Generate31_RiskRegister(outDir);
        Generate32_RemediationPlan(outDir);
        Generate33_TestMatrix(outDir);
        Generate34_BenchmarkReport(outDir);
        Generate35_SecurityAttackMatrix(outDir);
        Generate36_ChaosResults(outDir);
        Generate37_MutationResults(outDir);
        Generate38_ProductionReadiness(outDir);
        Generate39_ArchitecturalDecisions(outDir);
        Generate40_FinalVerdict(outDir);

        GenerateMachineReadableFindings(outDir);

        Console.WriteLine("All 41 Markdown reports and 5 machine-readable files successfully generated.");
    }

    private static void WriteFile(string outDir, string fileName, string content)
    {
        string path = Path.Combine(outDir, fileName);
        File.WriteAllText(path, content, Encoding.UTF8);
        Console.WriteLine($" -> Generated: {fileName} ({content.Length:N0} bytes)");
    }

    // Generator methods will be defined in partial classes / modules
}
