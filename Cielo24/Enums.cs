using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cielo24
{
    public enum TaskType
    {
        JobCreated,
        JobDeleted,
        JobAddMedia,
        JobAddTranscript,
        JobPerformTranscription,
        JobPerformPremiumSync,
        JobUpdateElementlist,
        JobGetTranscript,
        JobGetCaption,
        JobGetElementlist
    }

    public enum ErrorType
    {
        LoginInvalid,
        AccountExists,
        AccountDoesNotExist,
        AccountUnprivileged,
        BadApiToken,
        InvalidQuery,
        InvalidOption,
        InvalidUrl,
        MissingParameter,
        NotImplemented,
        ItemNotFound,
        InvalidReturnHandlers,
        NotParentAccount,
        NoChildrenFound,
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

    public enum Priority
    {
        Economy,
        Standard,
        PRIORITY,
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
        [Description("word")]
        Word,
        [Description("punctuation")]
        Punctuation,
        [Description("sound")]
        Sound
    }

    public enum Tag
    {
        Unknown,
        Inaudible,
        Crosstalk,
        Music,
        Noise,
        Laugh,
        Cough,
        Foreign,
        BlankAudio,
        Applause,
        Bleep,
        Guessed,
        EndsSentence
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
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            var readerValue = reader.Value.ToString().ToUpper();
            if (readerValue.Equals("STANDARD"))
            {
                return Fidelity.Premium;
            }
            return readerValue.Equals("HIGH")
                ? Fidelity.Professional
                : base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }

    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
                return value.ToString();
            var field = type.GetField(name);
            if (field == null)
                return value.ToString();
            var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return desc != null ? desc.Description : value.ToString();
        }
    }
}