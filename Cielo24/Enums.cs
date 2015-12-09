using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cielo24
{
    public static class TaskType
    {
        public const string JobCreated = "JOB_CREATED";
        public const string JobDeleted = "JOB_DELETED";
        public const string JobAddMedia = "JOB_ADD_MEDIA";
        public const string JobAddTranscript = "JOB_ADD_TRANSCRIPT";
        public const string JobPerformTranscription = "JOB_PERFORM_TRANSCRIPTION";
        public const string JobPerformPremiumSync = "JOB_PERFORM_PREMIUM_SYNC";
        public const string JobUpdateElementlist = "JOB_UPDATE_ELEMENTLIST";
        public const string JobGetTranscript = "JOB_GET_TRANSCRIPT";
        public const string JobGetCaption = "JOB_GET_CAPTION";
        public const string JobGetElementlist = "JOB_GET_ELEMENTLIST";
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum ErrorType
    {
        [Description("LOGIN_INVALID")]
        LoginInvalid,
        [Description("ACCOUNT_EXISTS")]
        AccountExists,
        [Description("ACCOUNT_DOES_NOT_EXIST")]
        AccountDoesNotExist,
        [Description("ACCOUNT_UNPRIVILEGED")]
        AccountUnprivileged,
        [Description("BAD_API_TOKEN")]
        BadApiToken,
        [Description("INVALID_QUERY")]
        InvalidQuery,
        [Description("INVALID_OPTION")]
        InvalidOption,
        [Description("INVALID_URL")]
        InvalidUrl,
        [Description("MISSING_PARAMETER")]
        MissingParameter,
        [Description("NOT_IMPLEMENTED")]
        NotImplemented,
        [Description("ITEM_NOT_FOUND")]
        ItemNotFound,
        [Description("INVALID_RETURN_HANDLERS")]
        InvalidReturnHandlers,
        [Description("NOT_PARENT_ACCOUNT")]
        NotParentAccount,
        [Description("NO_CHILDREN_FOUND")]
        NoChildrenFound,
        [Description("UNHANDLED_ERROR")]
        UnhandledError
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum JobStatus
    {
        [Description("Authorizing")]
        Authorizing,
        [Description("Pending")]
        Pending,
        [Description("In Process")]
        InProcess,
        [Description("Complete")]
        Complete,
        [Description("Reviewing")]
        Reviewing,
        [Description("Media Failure")]
        MediaFailure
    }

    public enum JobPriority
    {
        Economy,
        Standard,
        Priority,
        Critical
    }

    [JsonConverter(typeof(FidelityToEnumConverter))]
    public enum Fidelity
    {
        Mechanical,
        Premium,
        Professional
    }

    public enum CaptionFormat
    {
        Srt,
        Sbv,
        Scc,
        Dfxp,
        Qt,
        Transcript,
        Twx,
        Tpm,
        WebVtt,
        Echo
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum TokenType
    {
        [Description("WORD")]
        Word,
        [Description("PUNCTIATION")]
        Punctuation,
        [Description("SOUND")]
        Sound
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum Tag
    {
        [Description("UNKNOWN")]
        Unknown,
        [Description("INAUDIBLE")]
        Inaudible,
        [Description("CROSSTALK")]
        Crosstalk,
        [Description("MUSIC")]
        Music,
        [Description("NOISE")]
        Noise,
        [Description("LAUGH")]
        Laugh,
        [Description("COUGH")]
        Cough,
        [Description("FOREIGN")]
        Foreign,
        [Description("BLANK_AUDIO")]
        BlankAudio,
        [Description("APPLAUSE")]
        Applause,
        [Description("BLEEP")]
        Bleep,
        [Description("GUESSED")]
        Guessed,
        [Description("ENDS_SENTENCE")]
        EndsSentence,
        [Description("SOUND")]
        Sound
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum SpeakerId
    {
        [Description("no")]
        No,
        [Description("number")]
        Number,
        [Description("name")]
        Name
    }

    public enum SpeakerGender
    {
        Unknown,
        Male,
        Female
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum Case {
        [Description("upper")]
        Upper,
        [Description("lower")]
        Lower,
        [Description("")]
        Unchanged
    }

    public enum LineEnding
    {
        Unix,
        Windows,
        Osx
    }

    public enum CustomerApprovalStep
    {
        Translation,
        Return
    }

    public enum CustomerApprovalTool
    {
        Amara,
        Cielo24
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum Language
    {
        [Description("en")]
        English,
        [Description("fr")]
        French,
        [Description("es")]
        Spanish,
        [Description("de")]
        German,
        [Description("cmn")]
        MandarinChinese,
        [Description("pt")]
        Portuguese,
        [Description("jp")]
        Japanese,
        [Description("ar")]
        Arabic,
        [Description("ko")]
        Korean,
        [Description("zh")]
        TraditionalChinese,
        [Description("hi")]
        Hindi,
        [Description("it")]
        Italian,
        [Description("ru")]
        Russian,
        [Description("tr")]
        Turkish,
        [Description("he")]
        Hebrew
    }

    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum JobDifficulty
    {
        [Description("Good")]
        Good,
        [Description("Bad")]
        Bad,
        [Description("Unknown")]
        Unknown
    }

    [Flags]
    [JsonConverter(typeof(DescriptionToEnumConverter))]
    public enum Metric
    {
        [Description("billable_minutes_total")]
        BillableMinutesTotal = 1,
        [Description("billable_minutes_mechanical")]
        BillableMinutesMechanical = 2,
        [Description("billable_minutes_premium")]
        BillableMinutesPremium = 4,
        [Description("billable_minutes_professional")]
        BillableMinutesProfessional = 8,
        [Description("billable_minutes_foreign_transcription")]
        BillableMinutesForeignTranscription = 16,
        [Description("billable_minutes_translation")]
        BillableMinutesTranslation = 32,
        [Description("billable_english_transcription")]
        BillableMinutesEnglishTranscription = 64
    }
    
    /* Converts description into enum */
    public class DescriptionToEnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Nullable enums will cause a lot of trouble downstream if not converted to the underlying type
            if (Nullable.GetUnderlyingType(objectType) != null)
            {
                objectType = Nullable.GetUnderlyingType(objectType);
            }
            foreach (var val in objectType.GetEnumValues())
            {
                var name = Enum.GetName(objectType, val);
                var field = objectType.GetField(name);
                var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                var description = desc.Description.ToLower();
                var readerValue = reader.Value.ToString().ToLower();
                if (description.Equals(readerValue))
                {
                    return val;
                }
            }
            // If could not convert
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }

    /* Converts Fidelity into enum (for mapping purposes) */
    public class FidelityToEnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            var readerValue = reader.Value.ToString().ToUpper();
            if (readerValue.Equals("STANDARD"))
                return Fidelity.Premium;

            if (readerValue.Equals("HIGH"))
                return Fidelity.Professional;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }

    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
                return value.ToString().ToUpper();
            var field = type.GetField(name);
            if (field == null)
                return value.ToString();
            var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return desc != null ? desc.Description : value.ToString().ToUpper();
        }

        public static List<string> ToStringList(this Enum flags)
        {
            return (from Enum flag in Enum.GetValues(flags.GetType())
                    where flags.HasFlag(flag)
                    select flag.GetDescription()).ToList();
        }
    }
}