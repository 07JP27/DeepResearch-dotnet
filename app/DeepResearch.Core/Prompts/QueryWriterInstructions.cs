namespace DeepResearch.Core;

internal static partial class Prompts
{
    internal static readonly string QueryWriterInstructions = """
        Your goal is to generate a targeted web search query.

        <CONTEXT>
        Current date: {0}
        Please ensure your queries account for the most current information available as of this date.
        </CONTEXT>

        <TOPIC>
        {1}
        </TOPIC>

        <FORMAT>
        Format your response as a JSON object with ALL three of these exact keys:
           - "Query": The actual search query string
           - "Rationale": Brief explanation of why this query is relevant
        </FORMAT>

        <EXAMPLE>
        Example output:
        {{
            "Query": "machine learning transformer architecture explained",
            "Rationale": "Understanding the fundamental structure of transformer models"
        }}
        </EXAMPLE>

        Provide your response in JSON format. Do not include any tags or backticks. Only return
        Json like in the example:
        """;
}
