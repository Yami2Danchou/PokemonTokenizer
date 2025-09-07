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
    static HashSet<string> decisions = new HashSet<string> { "Pick a Pokémon:", "use Pokeball", "Got away safely!" };
    static HashSet<string> outcomes = new HashSet<string>
    {
        "It's super effective!", "It's not very effective!", "A critical hit!",
        "The opponent is paralyzed!", "The opponent is asleep!",
        "The opponent is poisoned!", "The attack hit normally."
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
        Console.WriteLine("=== Pokémon Grammar (CFG) ===\n");

        Console.WriteLine("<Game> → <Trainer> <Encounter> <Decision>");
        Console.WriteLine("<Trainer> → " + string.Join(" | ", trainers));
        Console.WriteLine("<Encounter> → \"sees a wild\" <WildPokemon>");
        Console.WriteLine("<WildPokemon> → " + string.Join(" | ", wildPokemon));
        Console.WriteLine("<Decision> → " + string.Join(" | ", decisions));
        Console.WriteLine("<Pokedex> → " + string.Join(" | ", pokedex));
        foreach (var mon in skills.Keys)
            Console.WriteLine($"<{mon}_Skills> → " + string.Join(" | ", skills[mon]));
        Console.WriteLine("<BattleOutcome> → " + string.Join(" | ", outcomes));
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Enter a Pokémon input (or type EXIT to quit):");
            string input = Console.ReadLine();

            if (input.Trim().ToUpper() == "EXIT")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            List<Token> tokenList = Tokenize(input);

            if (tokenList.Count == 0)
            {
                Console.WriteLine("Error: No valid tokens found. Please try again.\n");
                continue;
            }

            if (!Validate(tokenList))
            {
                Console.WriteLine("Error: Input does not match the grammar. Please try again.\n");
                continue;
            }

            Console.WriteLine("\nPhase 1: CFG-based classification");
            foreach (var token in tokenList)
                Console.WriteLine($"{token.Value} → {token.Type}");

            Console.WriteLine("\nPhase 2: Derivation (leftmost derivation)");
            Derive(tokenList);

            Console.WriteLine("\nInput accepted!\n");
        }
    }

    static List<Token> Tokenize(string input)
    {
        List<Token> tokenList = new List<Token>();
        string[] words = input.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            // Multi-word tokens: Decisions
            if (i + 1 < words.Length)
            {
                string twoWords = word + " " + words[i + 1];
                if (decisions.Contains(twoWords))
                {
                    tokenList.Add(new Token(twoWords, "<Decision>"));
                    i++;
                    continue;
                }
            }

            // Multi-word tokens: Outcomes
            bool matchedOutcome = false;
            foreach (var outcome in outcomes)
            {
                string[] outcomeWords = outcome.Split(' ');
                if (words.Skip(i).Take(outcomeWords.Length).SequenceEqual(outcomeWords))
                {
                    tokenList.Add(new Token(outcome, "<BattleOutcome>"));
                    i += outcomeWords.Length - 1;
                    matchedOutcome = true;
                    break;
                }
            }
            if (matchedOutcome) continue;

            // Single-word tokens
            if (trainers.Contains(word))
                tokenList.Add(new Token(word, "<Trainer>"));
            else if (wildPokemon.Contains(word))
                tokenList.Add(new Token(word, "<WildPokemon>"));
            else if (pokedex.Contains(word))
                tokenList.Add(new Token(word, "<Pokedex>"));
            else
            {
                foreach (var mon in skills.Keys)
                {
                    if (skills[mon].Contains(word))
                    {
                        tokenList.Add(new Token(word, $"<{mon}_Skills>"));
                        break;
                    }
                }
            }
        }

        return tokenList;
    }

    static bool Validate(List<Token> tokenList)
    {
        if (tokenList.Count < 3) return false;
        if (!tokenList.Any(t => t.Type == "<Trainer>")) return false;
        if (!tokenList.Any(t => t.Type == "<WildPokemon>")) return false;
        if (!tokenList.Any(t => t.Type == "<Decision>")) return false;

        return true;
    }

    static void Derive(List<Token> tokenList)
    {
        Console.WriteLine("<Game>");
        Console.WriteLine("⇒ <Trainer> <Encounter> <Decision>");

        // Trainer
        Token trainer = tokenList.FirstOrDefault(t => t.Type == "<Trainer>");
        Console.WriteLine($"⇒ {trainer.Value} <Encounter> <Decision>");

        // Encounter
        Token encounter = tokenList.FirstOrDefault(t => t.Type == "<WildPokemon>");
        Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} <Decision>");

        // Decision
        Token decision = tokenList.FirstOrDefault(t => t.Type == "<Decision>");
        if (decision.Value == "Pick a Pokémon:")
        {
            Token poke = tokenList.FirstOrDefault(t => t.Type == "<Pokedex>");
            Token skill = tokenList.FirstOrDefault(t => t.Type.Contains("_Skills"));
            var battleOutcomes = tokenList.Where(t => t.Type == "<BattleOutcome>").ToList();

            Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} Pick a Pokémon: <Pokedex> <BattleOutcomeList>");
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} Pick a Pokémon: {poke.Value} <BattleOutcomeList>");
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} Pick a Pokémon: {poke.Value} {skill.Value} <BattleOutcomeList>");

            string remainingOutcomes = string.Join(" ", battleOutcomes.Select(b => b.Value));
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} Pick a Pokémon: {poke.Value} {skill.Value} {remainingOutcomes}");
        }
        else
        {
            Console.WriteLine($"⇒ {trainer.Value} sees a wild {encounter.Value} {decision.Value}");
        }
    }
}
