using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;


namespace Teemonster
{
    class Program
    {
        static void Main(string[] args)
        {Loading.OnLoadingComplete += LoadingOnLoadingComplete;}

        
        public static  AIHeroClient User = Player.Instance;

        public static Menu Drawnigs, ComboMenu, KSMenu, HarassMenu, Auto, menu;
        private static Spell.Targeted Q;
        private static Spell.Active W;
        private static Spell.Skillshot R;

        

        private static void LoadingOnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Teemo")
            {
                return;
            }
            
            Game.OnTick += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Player.LevelSpell(SpellSlot.Q);
            Player.DoEmote(Emote.Joke);

            Bootstrap.Init(null);

            Q = new Spell.Targeted(SpellSlot.Q, spellRange: 680);

            W = new Spell.Active(SpellSlot.W);

            R = new Spell.Skillshot(SpellSlot.R, 0, SkillShotType.Circular, 0, 1000, 135);

            

            menu = MainMenu.AddMenu("Teemonster", "harigaren");

            menu.AddLabel("TEEMONSTER");
            menu.AddSeparator();
            menu.AddLabel("Obrigado por usar o addon Teemonster. Boa sorte!");
            menu.AddSeparator();
            menu.AddLabel("Versao: 1.1.0.0   19/07/2016");
            menu.AddSeparator();
            menu.AddLabel("Por favor reportar qualquer bug no forum do elobuddy. E deixe sua sugestão.");
            menu.AddSeparator();
            menu.AddLabel("Addon criado por: Evilazio");




            ComboMenu = menu.AddSubMenu("Combo", "comboMenu");
            HarassMenu = menu.AddSubMenu("Harass", "harassMenu");
            KSMenu = menu.AddSubMenu("Kill Steal", "KSMenu");
            Drawnigs = menu.AddSubMenu("Drawnigs", "drawnigs");
            Auto = menu.AddSubMenu("Auto Skill", "auto");

            ComboMenu.AddGroupLabel("CONFIGURAR COMBO:");
            ComboMenu.Add("qCombo", new CheckBox("Usar (Q) no combo"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("wCombo", new CheckBox("Usar (W) no combo"));
            ComboMenu.Add("wManaCombo", new Slider("Mana minima necessaria para o W: {0}", 25, 0, 100));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rCombo", new CheckBox("Usar (R) no combo"));
            ComboMenu.Add("rComboQnt", new Slider("Manter pelomenos {0} cogumelo(s)", 0, 0, 3));
            ComboMenu.AddSeparator();

            HarassMenu.AddGroupLabel("CONFIGURAR HARASS:");
            HarassMenu.Add("qHarass", new CheckBox("Usar (Q) no Harass"));
            HarassMenu.AddSeparator();
            HarassMenu.Add("rHarass", new CheckBox("Usar (R) no Harass"));
            HarassMenu.Add("rHarassQnt", new Slider("Manter pelomenos {0} cogumelo(s)", 0, 0, 3));
            HarassMenu.AddSeparator();

            KSMenu.AddGroupLabel("CONFIGURAR KS:");
            KSMenu.Add("qKS", new CheckBox("Usar (Q) no KS"));
            KSMenu.AddSeparator();

            Drawnigs.AddGroupLabel("Drawnings ON/OFF:");
            Drawnigs.Add("qDraw", new CheckBox("Draw (Q)  AMARELO / YELLOW"));
            Drawnigs.AddSeparator();
            Drawnigs.Add("rDraw", new CheckBox("Draw (R)  AZUL / BLUE"));

            Auto.AddGroupLabel("Auto Skill:");
            Auto.Add("AutoONOFF", new CheckBox("AUTO SKILL ON/OFF"));
            Auto.AddSeparator();
            Auto.Add("invAuto", new CheckBox("AUTO SKILL INVISIVEL", false));
            Auto.AddSeparator();
            Auto.Add("qAuto", new CheckBox("Auto (Q) "));
            Auto.Add("qAutoMana", new Slider("Auto (Q) se a mana for maior que: {0} %", 40, 0, 100));
            Auto.AddSeparator();
            Auto.Add("rAuto", new CheckBox("Auto (R) "));
            Auto.Add("rAutoMana", new Slider("Auto (R) se a mana for maior que: {0} %", 40, 0, 100));
            Auto.AddSeparator();
            Auto.Add("rAutoQnt", new Slider("Auto (R) se o numero de cogumelos for maior que: {0} ", 1, 0, 3)); 
            Auto.AddSeparator();
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawnigs["qDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.FromArgb(60, 200, 200, 0), Radius = Q.Range, BorderWidth = 5f }.Draw(User.Position);
            }
            if (Drawnigs["rDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle { Color = Color.FromArgb(60, 0, 50, 200), Radius = R.Range, BorderWidth = 2f }.Draw(User.Position);
            }
            
                new Circle { Color = Color.FromArgb(30,200,200,200), Radius = 1100, BorderWidth = 1f }.Draw(User.Position);

            
        }

        

        public static void OnTick(EventArgs args)
        {
            var target = TargetSelector.GetTarget(1100, damageType: DamageType.Magical);

            Rquantidade();
            KillSteal(target);
            AutoSkill(target);

            if (R.Level == 0)
            {
                R.Range = 5;
            }
            else if (R.Level == 1)
            {
                R.Range = 400;
            }
            else if (R.Level == 2)
            {
                R.Range = 650;
            }
            else if (R.Level == 3)
            {
                R.Range = 900;
            }


            //foreach (var buff in Player.Instance.Buffs)
            //{
            //    Chat.Say("BuffName: {0}, Stacks: {1}", buff.Name, buff.Count);
            //}
            
            

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo(target);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass(target);
            }


            

        }

        private static void Combo(Obj_AI_Base target)
        {


            if (target == null)
            {
                return;
            }

         


            if (ComboMenu["qCombo"].Cast<CheckBox>().CurrentValue && 
                target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target);
                
            }
            


            if (ComboMenu["wCombo"].Cast<CheckBox>().CurrentValue && User.ManaPercent > ComboMenu["wManaCombo"].Cast<Slider>().CurrentValue &&
                User.Distance(target) >= User.AttackRange && User.Distance(target) < 1100)
            {
                W.Cast();
            }

            
            
            var Rpred = R.GetPrediction(target);
            if (ComboMenu["rCombo"].Cast<CheckBox>().CurrentValue && Rquantidade() > ComboMenu["rComboQnt"].Cast<Slider>().CurrentValue &&
                target.IsValidTarget(R.Range) && R.IsReady() && Rpred.HitChance >= HitChance.High)
            {
                R.Cast(target.Position);
                
            }
        }

        private static void Harass(Obj_AI_Base target)
        {
            
            
            if (target == null)
            {
                return;
            }

            
            if (HarassMenu["qHarass"].Cast<CheckBox>().CurrentValue &&
               target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target);
            }
            

            var Rpred = R.GetPrediction(target);
            if (HarassMenu["rHarass"].Cast<CheckBox>().CurrentValue && Rquantidade() > ComboMenu["rHarassQnt"].Cast<Slider>().CurrentValue &&
               target.IsValidTarget(R.Range) && R.IsReady() && Rpred.HitChance >= HitChance.High )
            {
                
                R.Cast(target.Position);
            }
            


        }

        private static void AutoSkill(Obj_AI_Base target)
        {
            if ( !Auto["invAuto"].Cast<CheckBox>().CurrentValue && User.HasBuff("camouflagestealth") )
            {
                
                return;
            }
            

            if (target == null)
            {
                return;
            }


            

            if (Auto["qAuto"].Cast<CheckBox>().CurrentValue && User.ManaPercent > Auto["qAutoMana"].Cast<Slider>().CurrentValue  &&
               target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (Auto["rAuto"].Cast<CheckBox>().CurrentValue && Rquantidade() > Auto["rAutoQnt"].Cast<Slider>().CurrentValue && User.ManaPercent > Auto["rAutoMana"].Cast<Slider>().CurrentValue &&
               target.IsValidTarget(R.Range) && R.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                R.Cast(target.Position);
            }
        }

        public static void KillSteal(Obj_AI_Base target)
        {
            
            if (target != null && GetQDamage(target) >= target.Health)
            {
                Q.Cast(target);
            }
        }

        public static double GetQDamage(Obj_AI_Base target)
        {
            if (Q.IsReady())
                return Player.Instance.GetSpellDamage(target, SpellSlot.Q, DamageLibrary.SpellStages.Default);
            return 0;
        }


        public static int Rquantidade()
        {
            SpellDataInst Rinfo = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R);
            if (R.Level > 0 && R.IsReady())
            {
                return Rinfo.Ammo;
            }
            else
            {
                return 0;
            }
        }

    }
    
 }

