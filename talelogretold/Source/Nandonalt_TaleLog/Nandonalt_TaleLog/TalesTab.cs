using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Nandonalt_TaleLog
{
    public class TalesTab : MainTabWindow
    {
        private const float FactionColorRectSize = 15f;

        private const float FactionColorRectGap = 10f;

        private const float RowMinHeight = 80f;

        private const float LabelRowHeight = 50f;

        private const float TypeColumnWidth = 100f;

        private const float NameColumnWidth = 220f;

        private const float RelationsColumnWidth = 100f;

        private const float NameLeftMargin = 15f;

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight;
        public bool showColors = true;
        public bool enableDeath = true;
        public bool reverseOrder = false;
        public bool enableVomit = false;
        public String filter = "";
        public int ticker = 0;

   
        public List<String> tales = new List<String>();

        public String outputFile;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(1000f, 500f);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            buildTales();
            filter = "";
        }



        public override void DoWindowContents(Rect fillRect)
        {
           //base.DoWindowContents(fillRect);
            base.LateWindowOnGUI(fillRect);
            // if(ticker >= 1500)
            // {
            //    buildTales();
            //    ticker = 0;
            // }
            //ticker++;
            Text.Font = GameFont.Medium;
            Widgets.Label(fillRect, "Tales");
            Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);


           Rect position2 = new Rect(100f, 0f, 110F, 30F);
            //Widgets.CheckboxLabeled(position2, "Reverse", ref this.reverseOrder);
            if(Widgets.ButtonText(position2, "Invert Order"))
            {
                this.tales.Reverse();
            }
            
           
            Rect position3 = new Rect(280f, 5f, 110F, 30F);
            Widgets.Label(position3, "Filter");

            Rect position4 = new Rect(320f, 0f, 110F, 30F);
            filter = Widgets.TextField(position4, filter);

            Rect position5 = new Rect(500f, 0f, 110F, 30F);
            Widgets.CheckboxLabeled(position5, "Show Colors", ref this.showColors);

            Text.Font = GameFont.Tiny;
            Rect position6 = new Rect(615f, 0f, 300f, 30f);
            Widgets.Label(position6, "Written to " + outputFile);
            Text.Font = GameFont.Small;


            Rect rect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);

            Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect);

            float num = 0f;

            foreach (String tale in this.tales)
            {
                bool show = false;
                if(filter == "")
                {
                    show = true;
                }
                else if (tale.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    show = true;
                }

                if (show)
                {
                    GUI.color = Color.white;
                    if (showColors)
                    {
                        if (tale.Contains("Recruit") || tale.Contains("nimal") || tale.Contains("Hunted")) GUI.color = Color.green;
                        if (tale.Contains("Traded") || tale.Contains("Struck")) GUI.color = Color.gray;
                        if (tale.Contains("risoner") || tale.Contains("aked") || tale.Contains("Gave up")) GUI.color = Color.yellow;
                        if (tale.Contains("Marriage") || tale.Contains("Breakup") || tale.Contains("lover")) GUI.color = Color.magenta;
                        if (tale.Contains("Death") || tale.Contains("Kidnap") || tale.Contains("Berserk") || tale.Contains("Kill") || tale.Contains("kill") || tale.Contains("Raid") || tale.Contains("Human")) GUI.color = Color.red;
                        if (tale.Contains("Research") || tale.Contains("Landed") || tale.Contains("Surgery")) GUI.color = Color.cyan;
                    }
                    Rect rect2 = new Rect(0f, num, rect.width, 30f);
                    if (Mouse.IsOver(rect2))
                    {
                        GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    }
                    Widgets.Label(rect2, tale);
                    num += 30f;
                }

            }
            
            GUI.color = Color.white;
            if (Event.current.type == EventType.Layout)
            {
                this.scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        public void buildTales()
        {
            this.tales.Clear();   
            
            outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "rimworld_tales.txt");
            using (var output = new StreamWriter(outputFile, false))
            {

                foreach (Tale tale in Find.TaleManager.AllTalesListForReading)
                {
                    bool show = true;
                    String plus = ".";
                    String overrideName = "";

                    if (tale is Tale_SinglePawn)
                    {
                        Tale_SinglePawn tale2 = tale as Tale_SinglePawn;
                        if (tale2.pawnData == null)
                        {
                            show = false;
                        }
                        else if (tale2.pawnData.name == null || tale2.pawnData.name.ToString() == "" ||
                                 tale2.pawnData.name.ToStringFull == "" || tale2.pawnData.name.ToStringShort == "")
                        {
                            show = false;
                        }

                    }

                    if (tale.def == TaleDefOf.FinishedResearchProject)
                    {
                        Tale_SinglePawnAndDef tale2 = tale as Tale_SinglePawnAndDef;
                        plus = " - " + tale2.defData.def.LabelCap + ".";
                    }
                    else if (tale.def == TaleDefOf.Vomited && enableVomit == false)
                    {
                        show = false;
                    }

                    else if (tale.def == TaleDefOf.Recruited)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Joiner: " + tale2.secondPawnData.name + ".";
                        }
                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Recruiter: " + tale2.firstPawnData.name;
                        }
                    }


                    else if (tale is Tale_SinglePawnAndThing)
                    {
                        Tale_SinglePawnAndThing tale2 = tale as Tale_SinglePawnAndThing;
                        if (tale2.thingData != null)
                        {
                            plus = " - " + tale2.thingData.thingDef.LabelCap + ".";
                        }

                    }

                    else if (tale is Tale_DoublePawnKilledBy)
                    {
                        Tale_DoublePawnKilledBy tale2 = tale as Tale_DoublePawnKilledBy;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Killer: " + tale2.secondPawnData.name + ".";
                        }

                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Killer: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                        if (enableDeath == false)
                        {
                            show = false;
                        }
                    }


                    else if (tale.def == TaleDefOf.Hunted)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Prey: " + tale2.secondPawnData.name + ".";
                        }

                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Prey: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Hunter: " + tale2.firstPawnData.name;
                        }
                    }

                    else if (tale.def == TaleDefOf.KidnappedColonist)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Victim: " + tale2.secondPawnData.name + ".";
                        }
                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Kidnapper: " + tale2.firstPawnData.name;
                        }
                    }



                    else if (tale.def == TaleDefOf.DidSurgery)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Patient: " + tale2.secondPawnData.name + ".";
                        }
                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Patient: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                    }

                    else if (tale.def == TaleDefOf.KilledColonist)
                    {
                        //show = false;
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Colonist: " + tale2.secondPawnData.name + ".";
                        }
                        if (tale2.firstPawnData != null && !tale2.firstPawnData.kind.RaceProps.Humanlike)
                        {
                            overrideName = " - Killer: " + tale2.firstPawnData.kind.LabelCap;
                        }

                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Killer: " + tale2.firstPawnData.name;
                        }
                    }


                    else if (tale.def == TaleDefOf.KilledBy)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Enemy: " + tale2.secondPawnData.name + ".";
                        }
                        if (enableDeath == false)
                        {
                            show = false;
                        }
                    }

                    else if (tale.def == TaleDefOf.TradedWith)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Trader: " + tale2.secondPawnData.name + ".";
                        }
                    }

                    else if (tale.def == TaleDefOf.BecameLover || tale.def == TaleDefOf.Breakup ||
                             tale.def == TaleDefOf.Marriage)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " and " + tale2.secondPawnData.name + ".";
                        }
                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = ": " + tale2.firstPawnData.name + "";
                        }
                    }


                    else if (tale.def == TaleDefOf.Captured || tale.def == TaleDefOf.ExecutedPrisoner ||
                             tale.def == TaleDefOf.SoldPrisoner)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Animal: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Prisoner: " + tale2.secondPawnData.name + ".";
                        }

                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Warden: " + tale2.firstPawnData.name;
                        }
                    }

                    else if (tale.def == TaleDefOf.TamedAnimal || tale.def == TaleDefOf.TrainedAnimal ||
                             tale.def == TaleDefOf.BondedWithAnimal)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Animal: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Animal: " + tale2.secondPawnData.name + ".";
                        }

                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Handler: " + tale2.firstPawnData.name;
                        }
                    }
                    else if (tale.def == TaleDefOf.KilledColonyAnimal)
                    {
                        Tale_DoublePawn tale2 = tale as Tale_DoublePawn;
                        if (tale2.secondPawnData != null && !tale2.secondPawnData.kind.RaceProps.Humanlike)
                        {
                            plus = " - Animal: " + tale2.secondPawnData.kind.LabelCap + ".";
                        }
                        if (tale2.secondPawnData != null && tale2.secondPawnData.name != null)
                        {
                            plus = " - Animal: " + tale2.secondPawnData.name + ".";
                        }

                        if (tale2.firstPawnData != null && tale2.firstPawnData.name != null)
                        {
                            overrideName = " - Killer: " + tale2.firstPawnData.name;
                        }
                    }


                    if (show)
                    {
                        /// if (num - this.scrollPosition.y + 30f >= 0f && num - this.scrollPosition.y <= rect.height)
                        //{
                        StringBuilder str = new StringBuilder();
                        Vector2 longitude = Vector2.zero;
                        if (tale.surroundings != null && tale.surroundings.tile >= 0)
                        {
                            longitude = Find.WorldGrid.LongLatOf(tale.surroundings.tile);
                        }
                        str.Append(GenDate.DateFullStringAt(tale.date, longitude) + ": ");
                        string taleStr = tale.ToString();
                        string taleStr2 = taleStr.Split(new[] {"(age"}, StringSplitOptions.None)[0];
                        string taleStr3 = taleStr2.Split(',')[0];
                        str.Append(taleStr3.Remove(0, taleStr2.IndexOf(':') + 2));
                        if (overrideName == "")
                        {
                            str.Append(plus);
                            this.tales.Add(str.ToString());
                            output.WriteLine(str.ToString());
                        }
                        else
                        {
                            string[] temp = str.ToString().Split(':');
                            string final = temp[0] + ":" + temp[1];
                            var outstr = final + overrideName + plus;
                            this.tales.Add(outstr);
                            output.WriteLine(outstr);
                        }
                    }
                }
            }
            Log.Message("wrote tale log to " + outputFile);
            this.tales.Reverse();
        }
    }
}


