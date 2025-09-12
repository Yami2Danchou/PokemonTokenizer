using System;
using System.Collections.Generic;
using System.Linq;

class PokemonCfg
{
    // === CFG Terminals ===
    // List of valid Trainers
    static HashSet<string> trainers = new HashSet<string> { "Ash", "Brock", "Misty" };

    // List of Wild Pokémon that can appear
    static HashSet<string> wildPokemon = new HashSet<string> { "Rattata", "Pidgey", "Zubat", "Eevee", "Ekans" };

    // Player’s Pokémon (Pokedex)
    static HashSet<string> pokedex = new HashSet<string> { "Pikachu", "Onix", "Staryu" };

    // Skills each Pokémon can use (Dictionary: Pokémon → list of skills)
    static Dictionary<string, HashSet<string>> skills = new Dictionary<string, HashSet<string>>()
    {
        {"Pikachu", new HashSet<string>{ "Thunderbolt", "Quick Attack" }},
        {"Onix", new HashSet<string>{ "Rock Throw", "Tackle" }},
        {"Staryu", new HashSet<string>{ "Water Gun", "Swift" }}
    };

    // Possible decisions during a battle
    static HashSet<string> decisions = new HashSet<string> { "Pick a Pokemon:", "use Pokeball", "Got away safely!" };

    // Possible battle outcomes
    static HashSet<string> battleOutcomes = new HashSet<string>
    {
        "It's super effective!",
        "It's not very effective!",
        "A critical hit!",
        "The opponent is paralyzed!",
        "The opponent is asleep!",
        "The opponent is poisoned!",
        "The attack hit normally."
    };

    // === Token Class ===
    // Represents a word/phrase in the input with its type (e.g., Trainer, Skill, Outcome)
    public class Token
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public Token(string value, string type)
        {
            Value = value;
            Type = type;
        }
    }

    static void Main()
    {
        Console.WriteLine("-Pokémon Grammar Guide (CFG)-\n");

        // === Grammar Guide ===
        // Printed so user can see the rules
        Console.WriteLine("<Game> ::= <Trainer> sees a wild <WildPokemon> <Decision>");
        Console.WriteLine("<Trainer> ::= Ash | Brock | Misty");
        Console.WriteLine("<WildPokemon> ::= Rattata | Pidgey | Zubat | Eevee | Ekans");
        Console.WriteLine("<Decision> ::= Pick a Pokemon: <Fight> | use Pokeball | Got away safely!");
        Console.WriteLine("<Fight> ::= <Pokedex> use <Skill> <BattleOutcome>");
        Console.WriteLine("<Pokedex> ::= Pikachu | Onix | Staryu");

        // Print each Pokémon’s possible skills
        foreach (var mon in skills.Keys)
        {
            Console.WriteLine($"<{mon}> ::= <{mon}_Skills>");
            Console.WriteLine($"<{mon}_Skills> ::= {string.Join(" | ", skills[mon])}");
        }

        Console.WriteLine("<BattleOutcome> ::= It's super effective! | It's not very effective! | A critical hit! | The opponent is paralyzed! | The opponent is asleep! | The opponent is poisoned! | The attack hit normally.\n");

        // === Game Loop ===
        while (true)
        {
            Console.WriteLine("Enter a Pokémon input (or type ENDGAME to quit):");
            string input = Console.ReadLine();

            // Exit condition
            if (input.Trim().ToUpper() == "ENDGAME")
                break;

            // Step 1: Tokenize input
            List<Token> tokenList = Tokenize(input);

            if (tokenList.Count == 0)
            {
                Console.WriteLine("Error: No valid tokens found.\n");
                continue;
            }

            // Step 2: Validate input structure
            if (!Validate(tokenList))
            {
                Console.WriteLine("Input rejected.\n");
                continue;
            }

            // Step 3: Show tokens
            Console.WriteLine("\nPhase 1: Tokens");
            foreach (var token in tokenList)
                Console.WriteLine($"{token.Value} → {token.Type}");

            // Step 4: Show derivation (how input matches grammar)
            Console.WriteLine("\nPhase 2: Derivation");
            Derive(tokenList);

            Console.WriteLine("\nInput accepted!\n");
        }
    }

  // === Tokenizer ===
static List<Token> Tokenize(string input)
{
    List<Token> tokenList = new List<Token>();
    string[] words = input.Split(' ');

    for (int i = 0; i < words.Length; i++)
    {
        string word = words[i];

        // 1. Check for multi-word Battle Outcomes
        foreach (var outcome in battleOutcomes)
        {
            string[] outcomeWords = outcome.Split(' ');
            if (words.Skip(i).Take(outcomeWords.Length).SequenceEqual(outcomeWords))
            {
                tokenList.Add(new Token(outcome, "<BattleOutcome>"));
                i += outcomeWords.Length - 1;
                goto nextWord;
            }
        }

        // 2. Check for multi-word Decisions (any length, not just 2 words)
        foreach (var decision in decisions)
        {
            string[] decisionWords = decision.Split(' ');
            if (words.Skip(i).Take(decisionWords.Length).SequenceEqual(decisionWords))
            {
                tokenList.Add(new Token(decision, "<Decision>"));
                i += decisionWords.Length - 1;
                goto nextWord;
            }
        }

        // 3. Match single-word tokens
        if (trainers.Contains(word))
            tokenList.Add(new Token(word, "<Trainer>"));
        else if (wildPokemon.Contains(word))
            tokenList.Add(new Token(word, "<WildPokemon>"));
        else if (pokedex.Contains(word))
            tokenList.Add(new Token(word, "<Pokedex>"));
        else if (word == "use")
            tokenList.Add(new Token(word, "<UseKeyword>"));
        else
        {
            // 4. Check if word is a valid Pokémon skill
            foreach (var mon in skills.Keys)
            {
                if (skills[mon].Contains(word))
                {
                    tokenList.Add(new Token(word, $"<{mon}_Skills>"));
                    break;
                }
            }
        }

        nextWord: ;
    }

    return tokenList;
}


   // === Validator ===
// Ensures tokens follow grammar rules and Pokémon–skill relationship is correct
static bool Validate(List<Token> tokenList)
{
    // Must contain exactly ONE Trainer
    var trainersFound = tokenList.Where(t => t.Type == "<Trainer>").ToList();
    if (trainersFound.Count != 1)
    {
        Console.WriteLine("Validation error: Input must contain exactly one Trainer.");
        return false;
    }

    // Must contain exactly ONE Wild Pokémon
    var wildsFound = tokenList.Where(t => t.Type == "<WildPokemon>").ToList();
    if (wildsFound.Count != 1)
    {
        Console.WriteLine("Validation error: Input must contain exactly one Wild Pokémon.");
        return false;
    }

    // Must contain exactly ONE Decision
    var decisionsFound = tokenList.Where(t => t.Type == "<Decision>").ToList();
    if (decisionsFound.Count != 1)
    {
        Console.WriteLine("Validation error: Input must contain exactly one Decision.");
        return false;
    }

    Token decision = decisionsFound.First();

    // If user chose to fight
    if (decision.Value == "Pick a Pokemon:")
    {
        Token poke = tokenList.FirstOrDefault(t => t.Type == "<Pokedex>");
        Token useWord = tokenList.FirstOrDefault(t => t.Type == "<UseKeyword>");
        Token skill = tokenList.FirstOrDefault(t => t.Type.Contains("_Skills"));
        Token outcome = tokenList.FirstOrDefault(t => t.Type == "<BattleOutcome>");

        if (poke == null || useWord == null || skill == null || outcome == null)
        {
            Console.WriteLine("Validation error: Incomplete fight sequence.");
            return false;
        }

        // Extra check: ensure chosen skill matches the chosen Pokémon
        string pokeName = poke.Value;
        if (!skills.ContainsKey(pokeName) || !skills[pokeName].Contains(skill.Value))
        {
            Console.WriteLine($"Validation error: Skill '{skill.Value}' does not belong to {pokeName}.");
            return false;
        }
    }

    return true;
}


    // === Derivation ===
    // Shows how the input reduces from <Game> to final sentence
    static void Derive(List<Token> tokenList)
    {
        Token trainer = tokenList.First(t => t.Type == "<Trainer>");
        Token wild = tokenList.First(t => t.Type == "<WildPokemon>");
        Token decision = tokenList.First(t => t.Type == "<Decision>");

        Console.WriteLine("<Game>");
        Console.WriteLine("⇒ <Trainer> sees a wild <WildPokemon> <Decision>");
        Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} <Decision>");

        // Case 1: Battle
        if (decision.Value == "Pick a Pokemon:")
        {
            Token poke = tokenList.First(t => t.Type == "<Pokedex>");
            Token skill = tokenList.First(t => t.Type.Contains("_Skills"));
            Token outcome = tokenList.First(t => t.Type == "<BattleOutcome>");
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} Pick a Pokemon: {poke.Value} use {skill.Value} {outcome.Value}");
        }
        // Case 2: Pokéball
        else if (decision.Value == "use Pokeball")
        {
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} use Pokeball The pokemon was caught!");
        }
        // Case 3: Run away
        else
        {
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} Got away safely!");
        }
    }
}
