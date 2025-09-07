using System;
using System.Collections.Generic;
using System.Linq;

class PokemonCfg
{
    // === CFG Terminals ===
    static HashSet<string> trainers = new HashSet<string> { "Ash", "Brock", "Misty" };
    static HashSet<string> wildPokemon = new HashSet<string> { "Rattata", "Pidgey", "Zubat", "Eevee", "Ekans" };
    static HashSet<string> pokedex = new HashSet<string> { "Pikachu", "Onix", "Staryu" };
    static Dictionary<string, HashSet<string>> skills = new Dictionary<string, HashSet<string>>()
    {
        {"Pikachu", new HashSet<string>{ "Thunderbolt", "Quick Attack" }},
        {"Onix", new HashSet<string>{ "Rock Throw", "Tackle" }},
        {"Staryu", new HashSet<string>{ "Water Gun", "Swift" }}
    };

    static HashSet<string> decisions = new HashSet<string> { "Pick a Pokemon:", "use Pokeball", "Got away safely!" };

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
        Console.WriteLine("<Game> ::= <Trainer> <Encounter> <Decision>");
        Console.WriteLine("<Trainer> ::= Ash | Brock | Misty");
        Console.WriteLine("<Encounter> ::= sees a wild <WildPokemon>");
        Console.WriteLine("<WildPokemon> ::= Rattata | Pidgey | Zubat | Eevee | Ekans");
        Console.WriteLine("<Decision> ::= Pick a Pokemon: <Fight> | use Pokeball | Got away safely!");
        Console.WriteLine("<Fight> ::= <Pokedex> use <Skill> <BattleOutcome>");
        Console.WriteLine("<Pokedex> ::= Pikachu | Onix | Staryu");

        foreach (var mon in skills.Keys)
        {
            Console.WriteLine($"<{mon}> ::= <{mon}_Skills>");
            Console.WriteLine($"<{mon}_Skills> ::= {string.Join(" | ", skills[mon])}");
        }

        Console.WriteLine("<BattleOutcome> ::= " + string.Join(" | ", battleOutcomes));
        Console.WriteLine();

        // === Game Loop ===
        while (true)
        {
            Console.WriteLine("Enter a Pokémon input (or type ENDGAME to quit):");
            string input = Console.ReadLine();

            if (input == null) break;
            if (input.Trim().ToUpper() == "ENDGAME")
                break;

            List<Token> tokenList = Tokenize(input);

            if (tokenList.Count == 0)
            {
                Console.WriteLine("Error: No valid tokens found.\n");
                continue;
            }

            if (!Validate(tokenList))
            {
                Console.WriteLine("Input rejected.\n");
                continue;
            }

            Console.WriteLine("\nPhase 1: Tokens");
            foreach (var token in tokenList)
                Console.WriteLine($"{token.Value} → {token.Type}");

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
            bool matched = false;

            // 1) Multi-word battle outcomes
            foreach (var outcome in battleOutcomes)
            {
                string[] outcomeWords = outcome.Split(' ');
                if (i + outcomeWords.Length - 1 < words.Length &&
                    words.Skip(i).Take(outcomeWords.Length).SequenceEqual(outcomeWords))
                {
                    tokenList.Add(new Token(outcome, "<BattleOutcome>"));
                    i += outcomeWords.Length - 1;
                    matched = true;
                    break;
                }
            }
            if (matched) continue;

            // 2) Multi-word decisions
            foreach (var dec in decisions)
            {
                string[] decWords = dec.Split(' ');
                if (i + decWords.Length - 1 < words.Length &&
                    words.Skip(i).Take(decWords.Length).SequenceEqual(decWords))
                {
                    tokenList.Add(new Token(dec, "<Decision>"));
                    i += decWords.Length - 1;
                    matched = true;
                    break;
                }
            }
            if (matched) continue;

            // 3) Multi-word skills (e.g., "Water Gun", "Quick Attack", "Rock Throw")
            foreach (var mon in skills.Keys)
            {
                foreach (var skillPhrase in skills[mon])
                {
                    string[] skillWords = skillPhrase.Split(' ');
                    if (i + skillWords.Length - 1 < words.Length &&
                        words.Skip(i).Take(skillWords.Length).SequenceEqual(skillWords))
                    {
                        tokenList.Add(new Token(skillPhrase, $"<{mon}_Skills>"));
                        i += skillWords.Length - 1;
                        matched = true;
                        break;
                    }
                }
                if (matched) break;
            }
            if (matched) continue;

            // 4) Single-word tokens
            if (trainers.Contains(word))
                tokenList.Add(new Token(word, "<Trainer>"));
            else if (wildPokemon.Contains(word))
                tokenList.Add(new Token(word, "<WildPokemon>"));
            else if (pokedex.Contains(word))
                tokenList.Add(new Token(word, "<Pokedex>"));
            else if (word == "use")
                tokenList.Add(new Token(word, "<UseKeyword>"));
            else if (decisions.Contains(word))
                tokenList.Add(new Token(word, "<Decision>"));
            else
            {
                // unknown single word — skip or could be extended
            }
        }

        return tokenList;
    }

    // === Validator ===
    static bool Validate(List<Token> tokenList)
    {
        if (!tokenList.Any(t => t.Type == "<Trainer>"))
        {
            Console.WriteLine("Validation error: Missing <Trainer> token.");
            return false;
        }
        if (!tokenList.Any(t => t.Type == "<WildPokemon>"))
        {
            Console.WriteLine("Validation error: Missing <WildPokemon> token.");
            return false;
        }

        Token decision = tokenList.FirstOrDefault(t => t.Type == "<Decision>");
        if (decision == null)
        {
            Console.WriteLine("Validation error: Missing <Decision> token.");
            return false;
        }

        if (decision.Value == "Pick a Pokemon:")
        {
            Token poke = tokenList.FirstOrDefault(t => t.Type == "<Pokedex>");
            if (poke == null)
            {
                Console.WriteLine("Validation error: Missing <Pokedex> (choose a Pokemon).");
                return false;
            }

            if (!tokenList.Any(t => t.Type == "<UseKeyword>"))
            {
                Console.WriteLine("Validation error: Missing 'use' keyword.");
                return false;
            }

            // --- NEW: ensure the chosen skill belongs to the chosen Pokedex Pokemon ---
            string expectedSkillType = $"<{poke.Value}_Skills>";
            Token matchingSkill = tokenList.FirstOrDefault(t => t.Type == expectedSkillType);

            if (matchingSkill == null)
            {
                // If there is any skill token but it doesn't match the pokemon, show a specific error
                Token anySkill = tokenList.FirstOrDefault(t => t.Type.Contains("_Skills"));
                if (anySkill != null)
                {
                    Console.WriteLine($"Validation error: Skill '{anySkill.Value}' does not belong to {poke.Value}.");
                }
                else
                {
                    Console.WriteLine("Validation error: Missing <Skill> (check skill name and spelling).");
                }
                return false;
            }

            if (!tokenList.Any(t => t.Type == "<BattleOutcome>"))
            {
                Console.WriteLine("Validation error: Missing <BattleOutcome> (e.g. 'It's super effective!').");
                return false;
            }
        }

        return true;
    }

    // === Derivation ===
    static void Derive(List<Token> tokenList)
    {
        Token trainer = tokenList.First(t => t.Type == "<Trainer>");
        Token wild = tokenList.First(t => t.Type == "<WildPokemon>");
        Token decision = tokenList.First(t => t.Type == "<Decision>");

        Console.WriteLine("<Game>");
        Console.WriteLine("⇒ <Trainer> <Encounter> <Decision>");
        Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} <Decision>");

        if (decision.Value == "Pick a Pokemon:")
        {
            Token poke = tokenList.First(t => t.Type == "<Pokedex>");
            Token skill = tokenList.First(t => t.Type == $"<{poke.Value}_Skills>");
            Token outcome = tokenList.First(t => t.Type == "<BattleOutcome>");
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} Pick a Pokemon: {poke.Value} use {skill.Value} {outcome.Value}");
        }
        else if (decision.Value == "use Pokeball")
        {
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} use Pokeball The pokemon was caught!");
        }
        else
        {
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {wild.Value} Got away safely!");
        }
    }
}
