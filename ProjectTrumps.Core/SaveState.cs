using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class SaveState
    {
        public static SaveState Instance { get; set; } = new SaveState();

        public List<DataCard> MainDeck { get; set; } = new List<DataCard>();
        public List<DataCard> AdditionalDeck { get; set; } = new List<DataCard>();
        public List<DataCard> FullDeck
        {
            get
            {
                if (AdditionalDeck != null && AdditionalDeck.Any())
                    return MainDeck
                        .Concat(AdditionalDeck)
                        .ToList();

                return MainDeck;
            }
        }


        public string MainCardsLocation { get; set; } = "full_deck_list.csv";
        public string AdditionalSavedCardsLocation { get; set; } = "additionalcards.csv";
        
        public void LoadCards(string path, List<DataCard> deck)
        {
            if (deck == null) return;

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var card = CardFactory.Instance.CreateCard(line.Split(','));
                    deck.Add(card);
                }
            }
        }

        public void LoadMainDeck()
        {
            LoadCards(MainCardsLocation, MainDeck);
        }
        public void LoadAdditionalDeck()
        {
            if (!string.IsNullOrEmpty(AdditionalSavedCardsLocation) && File.Exists(AdditionalSavedCardsLocation))
            {
                LoadCards(AdditionalSavedCardsLocation, AdditionalDeck);
            }
        }

        public void SaveCard(DataCard newCard)
        {
            var existing = AdditionalDeck.FirstOrDefault(p => p.Id == newCard.Id);

            if (existing != null) 
            {
                existing.Type = newCard.Type;
                existing.CurrentAttributes = CardFactory.Instance.CopyAttributes(newCard.CurrentAttributes);
            }
            else
            {
                AdditionalDeck.Add(newCard);
            }

            var savedCards = AdditionalDeck.Select(p => p.ToString()).ToList();

            using (var writer = new StreamWriter(AdditionalSavedCardsLocation))
            {
                foreach (var savedCard in savedCards)
                {
                    writer.WriteLine(savedCard);
                }

            }
        }
    }
}
