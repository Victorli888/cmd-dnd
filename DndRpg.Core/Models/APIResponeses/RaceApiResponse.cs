using System.Text.Json.Serialization;

namespace DndRpg.Core;

public class RaceApiResponse
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("speed")]
    public int Speed { get; set; }
    
    [JsonPropertyName("ability_bonuses")]
    public List<AbilityBonus> AbilityBonuses { get; set; } = new List<AbilityBonus>();
    
    [JsonPropertyName("age")]
    public string Age { get; set; }
    
    [JsonPropertyName("alignment")]
    public string Alignment { get; set; }
    
    [JsonPropertyName("size")]
    public string Size { get; set; }
    
    [JsonPropertyName("size_description")]
    public string SizeDescription { get; set; }
    
    [JsonPropertyName("starting_proficiencies")]
    public List<StartingProficiency> StartingProficiencies { get; set; } = new List<StartingProficiency>();
    
    [JsonPropertyName("languages")]
    public List<Language> Languages { get; set; } = new List<Language>();
    
    [JsonPropertyName("language_options")]
    public LanguageOptions LanguageOptions { get; set; }
    
    [JsonPropertyName("language_desc")]
    public string LanguageDesc { get; set; }
    
    [JsonPropertyName("traits")]
    public List<Trait> Traits { get; set; } = new List<Trait>();
    
    [JsonPropertyName("subraces")]
    public List<Subrace> Subraces { get; set; } = new List<Subrace>();
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class AbilityBonus
{
    [JsonPropertyName("ability_score")]
    public AbilityScoreReference AbilityScore { get; set; }
    
    [JsonPropertyName("bonus")]
    public int Bonus { get; set; }
}

public class AbilityScoreReference
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class StartingProficiency
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Language
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class LanguageOptions
{
    [JsonPropertyName("choose")]
    public int Choose { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("from")]
    public OptionSet From { get; set; }
}

public class OptionSet
{
    [JsonPropertyName("option_set_type")]
    public string OptionSetType { get; set; }
    
    [JsonPropertyName("options")]
    public List<Option> Options { get; set; }
}

public class Option
{
    [JsonPropertyName("option_type")]
    public string OptionType { get; set; }
    
    [JsonPropertyName("item")]
    public Language Item { get; set; }
}

public class Trait
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Subrace
{
    [JsonPropertyName("index")]
    public string Index { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
