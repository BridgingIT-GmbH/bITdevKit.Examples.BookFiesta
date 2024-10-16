﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Code={Code}")]
public sealed class Language : ValueObject
{
    private static readonly Dictionary<string, string> ValidCodes = new()
    {
        { "aa", "Afar" }, { "ab", "Abkhazian" }, { "af", "Afrikaans" }, { "ak", "Akan" }, { "sq", "Albanian" },
        { "am", "Amharic" }, { "ar", "Arabic" }, { "an", "Aragonese" }, { "hy", "Armenian" }, { "as", "Assamese" },
        { "av", "Avaric" }, { "ae", "Avestan" }, { "ay", "Aymara" }, { "az", "Azerbaijani" }, { "ba", "Bashkir" },
        { "bm", "Bambara" }, { "eu", "Basque" }, { "be", "Belarusian" }, { "bn", "Bengali" }, { "bh", "Bihari languages" },
        { "bi", "Bislama" }, { "bs", "Bosnian" }, { "br", "Breton" }, { "bg", "Bulgarian" }, { "my", "Burmese" },
        { "ca", "Catalan" }, { "ch", "Chamorro" }, { "ce", "Chechen" }, { "zh", "Chinese" }, { "cu", "Church Slavic" },
        { "cv", "Chuvash" }, { "kw", "Cornish" }, { "co", "Corsican" }, { "cr", "Cree" }, { "cs", "Czech" },
        { "da", "Danish" }, { "dv", "Divehi" }, { "nl", "Dutch" }, { "dz", "Dzongkha" }, { "en", "English" },
        { "eo", "Esperanto" }, { "et", "Estonian" }, { "ee", "Ewe" }, { "fo", "Faroese" }, { "fj", "Fijian" },
        { "fi", "Finnish" }, { "fr", "French" }, { "fy", "Western Frisian" }, { "ff", "Fulah" }, { "ka", "Georgian" },
        { "de", "German" }, { "gd", "Gaelic" }, { "ga", "Irish" }, { "gl", "Galician" }, { "gv", "Manx" },
        { "el", "Greek" }, { "gn", "Guarani" }, { "gu", "Gujarati" }, { "ht", "Haitian" }, { "ha", "Hausa" },
        { "he", "Hebrew" }, { "hz", "Herero" }, { "hi", "Hindi" }, { "ho", "Hiri Motu" }, { "hr", "Croatian" },
        { "hu", "Hungarian" }, { "ig", "Igbo" }, { "is", "Icelandic" }, { "io", "Ido" }, { "ii", "Sichuan Yi" },
        { "iu", "Inuktitut" }, { "ie", "Interlingue" }, { "ia", "Interlingua" }, { "id", "Indonesian" },
        { "ik", "Inupiaq" }, { "it", "Italian" }, { "jv", "Javanese" }, { "ja", "Japanese" }, { "kl", "Kalaallisut" },
        { "kn", "Kannada" }, { "ks", "Kashmiri" }, { "kr", "Kanuri" }, { "kk", "Kazakh" }, { "km", "Central Khmer" },
        { "ki", "Kikuyu" }, { "rw", "Kinyarwanda" }, { "ky", "Kirghiz" }, { "kv", "Komi" }, { "kg", "Kongo" },
        { "ko", "Korean" }, { "kj", "Kuanyama" }, { "ku", "Kurdish" }, { "lo", "Lao" }, { "la", "Latin" },
        { "lv", "Latvian" }, { "li", "Limburgan" }, { "ln", "Lingala" }, { "lt", "Lithuanian" }, { "lb", "Luxembourgish" },
        { "lu", "Luba-Katanga" }, { "lg", "Ganda" }, { "mk", "Macedonian" }, { "mh", "Marshallese" }, { "ml", "Malayalam" },
        { "mi", "Maori" }, { "mr", "Marathi" }, { "ms", "Malay" }, { "mg", "Malagasy" }, { "mt", "Maltese" },
        { "mn", "Mongolian" }, { "na", "Nauru" }, { "nv", "Navajo" }, { "nr", "Ndebele, South" }, { "nd", "Ndebele, North" },
        { "ng", "Ndonga" }, { "ne", "Nepali" }, { "nn", "Norwegian Nynorsk" }, { "nb", "Bokmål, Norwegian" },
        { "no", "Norwegian" }, { "ny", "Chichewa" }, { "oc", "Occitan" }, { "oj", "Ojibwa" }, { "or", "Oriya" },
        { "om", "Oromo" }, { "os", "Ossetian" }, { "pa", "Panjabi" }, { "fa", "Persian" }, { "pi", "Pali" },
        { "pl", "Polish" }, { "pt", "Portuguese" }, { "ps", "Pushto" }, { "qu", "Quechua" }, { "rm", "Romansh" },
        { "ro", "Romanian" }, { "rn", "Rundi" }, { "ru", "Russian" }, { "sg", "Sango" }, { "sa", "Sanskrit" },
        { "si", "Sinhala" }, { "sk", "Slovak" }, { "sl", "Slovenian" }, { "se", "Northern Sami" }, { "sm", "Samoan" },
        { "sn", "Shona" }, { "sd", "Sindhi" }, { "so", "Somali" }, { "st", "Sotho, Southern" }, { "es", "Spanish" },
        { "sc", "Sardinian" }, { "sr", "Serbian" }, { "ss", "Swati" }, { "su", "Sundanese" }, { "sw", "Swahili" },
        { "sv", "Swedish" }, { "ty", "Tahitian" }, { "ta", "Tamil" }, { "tt", "Tatar" }, { "te", "Telugu" },
        { "tg", "Tajik" }, { "tl", "Tagalog" }, { "th", "Thai" }, { "bo", "Tibetan" }, { "ti", "Tigrinya" },
        { "to", "Tonga" }, { "tn", "Tswana" }, { "ts", "Tsonga" }, { "tk", "Turkmen" }, { "tr", "Turkish" },
        { "tw", "Twi" }, { "ug", "Uighur" }, { "uk", "Ukrainian" }, { "ur", "Urdu" }, { "uz", "Uzbek" },
        { "ve", "Venda" }, { "vi", "Vietnamese" }, { "vo", "Volapük" }, { "cy", "Welsh" }, { "wa", "Walloon" },
        { "wo", "Wolof" }, { "xh", "Xhosa" }, { "yi", "Yiddish" }, { "yo", "Yoruba" }, { "za", "Zhuang" }, { "zu", "Zulu" }
    };

    private Language(string code)
    {
        this.Code = string.Intern(code.ToLowerInvariant());
    }

    public string Code { get; }

    public string Name => ValidCodes[this.Code];

    public static implicit operator string(Language language) => language.Code;

    public static Language Create(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("Language code cannot be null or empty.");
        }

        if (code.Length != 2)
        {
            throw new ArgumentException("Language code must be exactly two characters long.");
        }

        code = code.ToLowerInvariant();

        if (!ValidCodes.ContainsKey(code))
        {
            throw new ArgumentException($"Invalid language code: {code}");
        }

        return new Language(code);
    }

    public static IEnumerable<Language> GetAll()
    {
        return ValidCodes.Keys.Select(Create);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Code;
    }
}