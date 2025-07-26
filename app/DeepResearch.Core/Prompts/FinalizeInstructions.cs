namespace DeepResearch.Core;

internal static partial class Prompts
{
    internal static string FinalizeInstructions(List<string> summariesGathered)
    {
        if (summariesGathered == null || summariesGathered.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate finalize instructions with an empty list of summaries.");
        }

        return
        $"""
        - Your task is to synthesize the piecemeal researched summaries to create a coherent final report.
        - Your goal is to create a final report on the <TOPIC> submitted by the user with the information in the <SUMMARIES>.
        - Do not use any knowledge other than the provided <SUMMARIES>.
        - You will be provided with a list of summaries created during the research process.
        - Use the provided summaries to create a comprehensive final report.
        - Make sure your final report is clear and concise and captures the essence of the research conducted.
        - Generate your final report in the same language used in the <TOPIC>.

        <SUMMARIES>
        {string.Join('\n', summariesGathered.Select(s => $"<SUMMARY>{s}</SUMMARY>"))}
        </SUMMARIES>
        """;
    }
}
