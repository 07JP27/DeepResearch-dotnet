using System;

namespace DeepResearch.Core;

internal static partial class Prompts
{
    internal static readonly string ReflectionInstructions = """
        You are an expert research assistant analyzing a summary about {0}.

        <GOAL>
        1. Identify knowledge gaps or areas that need deeper exploration
        2. Generate a follow-up question that would help expand your understanding
        3. Focus on technical details, implementation specifics, or emerging trends that weren't fully covered
        </GOAL>

        <REQUIREMENTS>
        Ensure the follow-up question is self-contained and includes necessary context for web search.
        </REQUIREMENTS>

        <FORMAT>
        Format your response as a JSON object with these exact keys:
        - KnowledgeGap: Describe what information is missing or needs clarification
        - FollowUpQuery: Write a specific question to address this gap
        </FORMAT>

        <Task>
        Reflect carefully on the Summary to identify knowledge gaps and produce a follow-up query. Then, produce your output following this JSON format:
        {{
            "KnowledgeGap": "The summary lacks information about performance metrics and benchmarks",
            "FollowUpQuery": "What are typical performance benchmarks and metrics used to evaluate [specific technology]?"
        }}
        </Task>

        Provide your analysis in JSON format. Do not include any tags or backticks. Only return
        Json like in the example:
        """;
}
