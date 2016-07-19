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

        public static Menu Drawnigs, ComboMenu, KSMenu, HarassMenu, menu;
        private static Spell.Targeted Q;
        private static Spell.Active W;
        private static Spell.Skillshot R;

        

        private static void LoadingOnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Teemo")
            {
                return;
            }
            Bootstrap.Init(null);
            Game.OnTick += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

            
            Q = new Spell.Targeted(SpellSlot.Q, spellRange: 680);

            W = new Spell.Active(SpellSlot.W);

            R = new Spell.Skillshot(SpellSlot.R, 0, SkillShotType.Circular, 0, 1000, 135);

            

            menu = MainMenu.AddMenu("Teemonster", "harigaren");
            ComboMenu = menu.AddSubMenu("Combo", "comboMenu");
            HarassMenu = menu.AddSubMenu("Harass", "harassMenu");
            KSMenu = menu.AddSubMenu("KillSteal", "KSMenu");
            Drawnigs = menu.AddSubMenu("Drawnigs", "drawnigs");

            ComboMenu.AddGroupLabel("CONFIGURAR COMBO:");
            ComboMenu.Add("qCombo", new CheckBox("Usar (Q) no combo"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("wCombo", new CheckBox("Usar (W) no combo"));
            ComboMenu.Add("wManaCombo", new Slider("Mana minima necessaria para o W: {0}", 25, 0, 100));
            ComboMenu.AddSeparator();
            ComboMenu.Add("rCombo", new CheckBox("Usar (R) no combo"));
            ComboMenu.Add("rQntCombo", new Slider("Manter pelomenos {0} cogumelo(s)", 0, 0, 3));
            ComboMenu.AddSeparator();

            HarassMenu.AddGroupLabel("CONFIGURAR HARASS:");
            HarassMenu.Add("qHarass", new CheckBox("Usar (Q) no Harass"));
            HarassMenu.AddSeparator();
            HarassMenu.Add("rHarass", new CheckBox("Usar (R) no Harass"));
            HarassMenu.Add("rQntHarass", new Slider("Manter pelomenos {0} cogumelo(s)", 0, 0, 3));
            HarassMenu.AddSeparator();

            KSMenu.AddGroupLabel("CONFIGURAR KS:");
            KSMenu.Add("qKS", new CheckBox("Usar (Q) no KS"));
            KSMenu.AddSeparator();
            KSMenu.Add("wKS", new CheckBox("Usar (W) no KS"));
            KSMenu.Add("wManaKS", new Slider("Mana minima necessaria para o W: {0}", 40, 0, 100));
            KSMenu.AddSeparator();

            Drawnigs.AddGroupLabel("Drawnings ON/OFF:");
            Drawnigs.Add("qDraw", new CheckBox("Draw (Q)  AMARELO / YELLOW"));
            Drawnigs.AddSeparator();
            Drawnigs.Add("rDraw", new CheckBox("Draw (R)  AZUL / BLUE"));


            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            new Circle { Color = Color.FromArgb(60, 200, 200, 0), Radius = Q.Range, BorderWidth = 2f }.Draw(User.Position);
            new Circle { Color = Color.FromArgb(60, 0, 50, 200), Radius = R.Range, BorderWidth = 5f }.Draw(User.Position);
            new Circle { Color = Color.FromArgb(30,200,200,200), Radius = 1100, BorderWidth = 5f }.Draw(User.Position);
        }

        public static void OnTick(EventArgs args)
        {
            Rquantidade();
            KillSteal();
            

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



            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

        }

        private static void Combo()
        {
            

            var target = TargetSelector.GetTarget(1100, damageType: DamageType.Magical);

            if(target == null)
            {
                return;
            }


            if (ComboMenu["qCombo"].Cast<CheckBox>().CurrentValue && 
                target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target);
            }


            if (ComboMenu["wCombo"].Cast<CheckBox>().CurrentValue && User.ManaPercent > ComboMenu["wManaCombo"].Cast<Slider>().CurrentValue &&
                User.Distance(target) > User.AttackRange && User.Distance(target) < 1100)
            {
                W.Cast();
            }

            
            
            var Rpred = R.GetPrediction(target);
            if (ComboMenu["rCombo"].Cast<CheckBox>().CurrentValue && Rquantidade() > ComboMenu["rCombo"].Cast<Slider>().CurrentValue &&
                target.IsValidTarget(R.Range) && R.IsReady() && Rpred.HitChance >= HitChance.High)
            {
                R.Cast(target.Position);
                
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1100, damageType: DamageType.Magical);

            if (ComboMenu["qHarass"].Cast<CheckBox>().CurrentValue &&
               target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (ComboMenu["rHarass"].Cast<CheckBox>().CurrentValue && Rquantidade() > ComboMenu["rQntHarass"].Cast<Slider>().CurrentValue &&
               target.IsValidTarget(R.Range) && R.IsReady())
            {
                R.Cast(target.Position);
            }


        }

        public static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
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

