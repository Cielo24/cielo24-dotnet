using System.Collections.Generic;

namespace Cielo24.Options
{
    public class PerformTranscriptionOptions : BaseOptions
    {
        [QueryName("customer_approval_steps")]
        public List<CustomerApprovalStep> CustomerApprovalSteps { get; set; }
        [QueryName("customer_approval_tool")]
        public CustomerApprovalTool? CustomerApprovalTool { get; set; }
        [QueryName("custom_metadata")]
        public string CustomMetadata { get; set; }
        [QueryName("generate_media_intelligence_for_iwp")]
        public bool? GenerateMediaIntelligenceForIwp { get; set; }
        [QueryName("notes")]
        public string Notes { get; set; }
        [QueryName("return_iwp")]
        public List<Fidelity> ReturnIwp { get; set; }
        [QueryName("speaker_id")]
        public bool? SpeakerId { get; set; }

        public PerformTranscriptionOptions(List<CustomerApprovalStep> customerApprovalStep = null,
                                           CustomerApprovalTool? customerApprovalTool = null,
                                           string customMetadata = null,
                                           bool? generateMediaIntelligenceForIwp = null,
                                           string notes = null,
                                           List<Fidelity> returnIwp = null,
                                           bool? speakerId = null)
        {
            CustomerApprovalSteps = customerApprovalStep;
            CustomerApprovalTool = customerApprovalTool;
            CustomMetadata = customMetadata;
            GenerateMediaIntelligenceForIwp = generateMediaIntelligenceForIwp;
            Notes = notes;
            ReturnIwp = returnIwp;
            SpeakerId = speakerId;
        }
    }
}
