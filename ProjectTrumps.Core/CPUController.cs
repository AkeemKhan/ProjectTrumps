using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class CPUController
    {
        public CPUController() 
        {
            Morale = new CPUMoralaController(this, 100);
        }

        public List<int> Commands = new List<int>();
        public CPUMoralaController Morale { get; set; }
        public int DamageThreshold { get; set; }
        public int DamageTakenCounter { get; set; }
        public int ChangeAtNTimesDamageTaken { get; set; }
        public bool ChangeCard { get; set; }
    }

    public class CPUMoralaController
    {
        private CPUController _cpuController;
        public float CurrentMorale { get; private set; }
        public float MaxMorale { get; private set; }
        public bool Broken => CurrentMorale == 0;

        public CPUMoralaController(CPUController cpuController, float maxMorale) 
        {
            _cpuController = cpuController;
            MaxMorale = CurrentMorale = maxMorale;
        }

        public void EvaluateMorale(int healthBefore, int healthAfter, bool playerTurn, out BattleLog log)
        {
            float segment = (1f / 6f) * 100;
            log = new BattleLog();

            if (healthBefore - healthAfter >= _cpuController.DamageThreshold)
            {
                var diff = healthBefore - healthAfter;
                if (diff * 2 > _cpuController.DamageThreshold)
                {
                    float moraleDamage = 0;
                    // 6 / 2 + 1 = 4
                    // 4/6 = 66%
                    // 1/6 = 16.6%

                    // Heavy Morale Damage                
                    DamageMorale(segment * 4);

                    // aiMoraleDamage += ((aiChangeAtMoraleBreak / 2) + 1);
                    log.AddMessage(true, $"Inflicted SIGNIFICANT damage");
                }
                else if (!playerTurn)
                {
                    // aiMoraleDamage++;
                    DamageMorale(segment);
                    log.AddMessage(true, $"Inflicted considerable damage");
                }
                else
                {
                    // aiMoraleDamage = aiMoraleDamage + 2;
                    DamageMorale(segment*2);
                    log.AddMessage(true, $"Self-Inflicted considerable damage");
                }
            }
            else
            {                
                IncreaseMorale(segment);
            }

            if (Broken)
            {
                log.AddMessage(true, $"Opponenet has Broken - Morale - 0%");
            }
            else
            {
                log.AddMessage(true, $"Morale - {CurrentMorale.ToString("0")}%");
            }

            if (Broken || _cpuController.DamageTakenCounter == _cpuController.ChangeAtNTimesDamageTaken)
            {
                ResetMorale();
                _cpuController.DamageTakenCounter = 0;
                _cpuController.ChangeCard = true;
            }
        }

        public void IncreaseMorale(float morale)
        {
            CurrentMorale += morale;
            CurrentMorale = CurrentMorale > MaxMorale ? MaxMorale : CurrentMorale;
        }

        public void DamageMorale(float damage)
        {
            CurrentMorale -= damage;
            CurrentMorale = CurrentMorale < 0 ? 0 : CurrentMorale;
        }

        public void ResetMorale()
        {
            CurrentMorale = MaxMorale;
        }
    }
}
