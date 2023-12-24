using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using static RNC.Character;

namespace RNC
{
    public class Character : Script
    {
        public class HeadOverlayData
        {
            public enum Overlay
            {
                Blemishes = 0,
                FacialHair = 1,
                Eyebrows = 2,
                Ageing = 3,
                Makeup = 4,
                Blush = 5,
                Complexion = 6,
                SunDamage = 7,
                Lipstick = 8,
                MolesFreckles = 9,
                ChestHair = 10,
                BodyBlemishes = 11,
                AddBodyBlemishes = 12
            }
            public int OverlayID;
            public int Index;
            public float Opacity;
            public int FirstColor;
            public int SecondColor;

            public HeadOverlayData() { }
            public HeadOverlayData(Overlay overlay, int index, float opacity, int firstColor, int secondColor)
            {
                this.OverlayID = (int)overlay;
                this.Index = index;
                this.Opacity = opacity;
                this.FirstColor = firstColor;
                this.SecondColor = secondColor;
            }
        }
        public class FaceFeatureData
        {
            public int Index;
            public float Scale;
            public enum FaceFeature
            {
                NoseWidth = 0,
                NoseHeight = 1,
                NoseLength = 2,
                NoseBridge = 3,
                NoseTip = 4,
                NoseBridgeShift = 5,
                BrowHeight = 6,
                BrowWidth = 7,
                CheekboneHeight = 8,
                CheekboneWidth = 9,
                CheeksWidth = 10,
                Eyes = 11,
                Lips = 12,
                JawWidth = 13,
                JawHeight = 14,
                ChinLength = 15,
                ChinPosition = 16,
                ChinWidth = 17,
                ChinShape = 18,
                NeckWidth = 19
            }

            public FaceFeatureData() { }
            public FaceFeatureData(FaceFeature feature, float scale)
            {
                this.Index = (int)feature;
                this.Scale = scale;
            }
        }
        public class HairData
        {
            public int Index;
            public int Color;
            public int HighlightColor;

            public HairData() { }
            public HairData(int index, int color, int highlightColor)
            {
                Index = index;
                Color = color;
                HighlightColor = highlightColor;
            }
        }
        public class HeadBlendData
        {
            public int FirstParentID;
            public int SecondParentID;
            public int MixParentID;
            public int FirstSkinID;
            public int SecondSkinID;
            public int MixSkinID;
            public float ShapeMix;
            public float SkinMix;
            public float ThirdMix;
            public bool IsParent;

            public HeadBlendData() { }
            public HeadBlendData(int firstParentID, int secondParentID, int mixParentID, int firstSkinID, int secondSkinID, int mixSkinID, float shapeMix, float skinMix, float thirdMix, bool isParent)
            {
                FirstParentID = firstParentID;
                SecondParentID = secondParentID;
                MixParentID = mixParentID;
                FirstSkinID = firstSkinID;
                SecondSkinID = secondSkinID;
                MixSkinID = mixSkinID;
                ShapeMix = shapeMix;
                SkinMix = skinMix;
                ThirdMix = thirdMix;
                IsParent = isParent;
            }
        }

        public class PropData
        {
            public enum Prop
            {
                Hat = 0,
                Glasses = 1,
                Ears = 2,
                Watch = 6,
                Bracelet = 7
            }

            public int Index;
            public int Drawable;
            public int Texture;

            public PropData(int index, int drawable, int texture)
            {
                Index = index;
                Drawable = drawable;
                Texture = texture;
            }
            public PropData() { }
        }
        public class ComponentData
        {
            public enum Component
            {
                Head = 0,
                Masks = 1,
                HairStyles = 2,
                Torsos = 3,
                Legs = 4,
                BagsAndParachutes = 5,
                Shoes = 6,
                Accessories = 7,
                Undershirts = 8,
                BodyArmors = 9,
                Decals = 10,
                Tops = 11
            }

            public int Drawable;
            public int Texture;
            public int Palette;

            public ComponentData() { }
            public ComponentData(int drawable, int texture, int palette = 0)
            {
                Drawable = drawable;
                Texture = texture;
                Palette = palette;
            }
        }

        public class Appearance
        {
            public string Name;
            public bool IsMale;
            public int Team;
            public List<HeadOverlayData> HeadOverlays = new List<HeadOverlayData>();
            public List<FaceFeatureData> FaceFeatures = new List<FaceFeatureData>();
            public int EyeColor;
            public HairData Hair;
            public HeadBlendData HeadBlend;
            public List<string> HairOverlays = new List<string>();
            public List<string> Tattoos = new List<string>();
            public Appearance() { }
            public Appearance(string name)
            {
                Name = name;
                IsMale = true;
                Team = -1;
                EyeColor = 0;
                for(int i = 0; i < 13; i++)
                {
                    HeadOverlays.Add(new HeadOverlayData((HeadOverlayData.Overlay)i, -1, 1.0f, 0, 0));
                }
                for(int i = 0; i < 20; i++)
                {
                    FaceFeatures.Add(new FaceFeatureData((FaceFeatureData.FaceFeature)i, 0));
                }

                Hair = new HairData(0, 0, 0);
                HairOverlays = new List<string>();
                Tattoos = new List<string>();
                HeadBlend = new HeadBlendData(0, 0, 0, 0, 0, 0, 0, 0, 0, false);
            }
        }
        public class Outfit
        {
            public string Name;
            public string Icon;

            public List<ComponentData> Components = new List<ComponentData>();
            public List<PropData> Props = new List<PropData>();
            public bool IsMale;
            public int Team;
            public Outfit() { }
            public Outfit(string name, string icon, bool isMale, int team)
            {
                this.Name = name;
                this.Icon = icon;
                this.IsMale = isMale;
                this.Team = team;

                Components = GetDefaultOutfit(team, isMale);

                Props.Add(new PropData((int)PropData.Prop.Hat, -1, -1));
                Props.Add(new PropData((int)PropData.Prop.Glasses, -1, -1));
                Props.Add(new PropData((int)PropData.Prop.Watch, -1, -1));
                Props.Add(new PropData((int)PropData.Prop.Ears, -1, -1));
                Props.Add(new PropData((int)PropData.Prop.Bracelet, -1, -1));
            }

            private List<ComponentData> GetDefaultOutfit(int team, bool isMale)
            {
                List<ComponentData> components = new List<ComponentData>();

                for(int i = 0; i < 12; i++)
                {
                    components.Add(new ComponentData(0, 0, 0));
                }


                if (isMale)
                {
                    if(team == 0)
                    {
                        components[(int)ComponentData.Component.Undershirts] = new ComponentData(15, 0);
                        components[(int)ComponentData.Component.Torsos] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Tops] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Legs] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Shoes] = new ComponentData(1, 0);
                    }
                    else if(team == 1)
                    {
                        components[(int)ComponentData.Component.Undershirts] = new ComponentData(15, 0);
                        components[(int)ComponentData.Component.Torsos] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Tops] = new ComponentData(55, 0);
                        components[(int)ComponentData.Component.Legs] = new ComponentData(35, 0);
                        components[(int)ComponentData.Component.Shoes] = new ComponentData(25, 0);
                    }
                }
                else
                {
                    if (team == 0)
                    {
                        components[(int)ComponentData.Component.Undershirts] = new ComponentData(15, 0);
                        components[(int)ComponentData.Component.Torsos] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Tops] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Legs] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Shoes] = new ComponentData(0, 0);
                    }
                    else if (team == 1)
                    {
                        components[(int)ComponentData.Component.Undershirts] = new ComponentData(15, 0);
                        components[(int)ComponentData.Component.Torsos] = new ComponentData(0, 0);
                        components[(int)ComponentData.Component.Tops] = new ComponentData(48, 0);
                        components[(int)ComponentData.Component.Legs] = new ComponentData(34, 0);
                        components[(int)ComponentData.Component.Shoes] = new ComponentData(25, 0);
                    }
                }

                return components;
            }
        }

        public Appearance CurrentAppearance;
        public Outfit CurrentOutfit;
        public SavedAppearance SavedAppearance;
        public SavedOutfit SavedOutfit;

        public Character()
        {

        }
        public Character(Player player, SavedAppearance appearance = null, SavedOutfit outfit = null)
        {
            this.SavedAppearance = appearance;
            this.SavedOutfit = outfit;

            this.CurrentAppearance = ApplyAppearance(player, appearance, false);
            if (outfit == null)
                this.CurrentOutfit = ApplyOutfit(player, this.CurrentAppearance.IsMale, 1, null, false);
            else
            {
                Outfit _o = NAPI.Util.FromJson<Outfit>(outfit.Data);
                this.CurrentOutfit = ApplyOutfit(player, _o.IsMale, _o.Team, outfit, false);
            }

            player.SetData<Character>("CHARACTER:Character", this);

        }

        public async static Task DeleteAllCharacterData(Account acc)
        {
            using(MainDatabase context = ContextFactory.CreateContext())
            {
                List<SavedOutfit> outfits = await context.SavedOutfits.AsQueryable().Where(o => o.AccountID == acc.SQL_ID).ToListAsync();
                List<SavedAppearance> appearances = await context.SavedAppearances.AsQueryable().Where(o => o.AccountID == acc.SQL_ID).ToListAsync();
                CharacterSlot slot = await context.CharacterSlots.AsQueryable().FirstOrDefaultAsync(s => s.AccountID == acc.SQL_ID);
                if(slot != null)
                    context.CharacterSlots.Remove(slot);

                if(appearances != null)
                    context.SavedAppearances.RemoveRange(appearances);

                if(outfits != null)
                    context.SavedOutfits.RemoveRange(outfits);

                await context.SaveChangesAsync();
            }
        }
        public static void InitializeCharacter(Player player, Account acc)
        {
            try
            {
                int AccountSQLID = acc.SQL_ID;
                if (AccountSQLID == 0)
                {
                    NAPI.Util.ConsoleOutput($"Failed to init: acc SQLID is 0! {acc.SQL_ID} {acc.Username}");
                    return;
                }

                Character newCharacter = new Character(player, null, null);
                if(newCharacter == null)
                {
                    NAPI.Util.ConsoleOutput($"[CHARACTER EDITOR] New character was null during char creation!");
                }

                SavedAppearance firstAppearance = new SavedAppearance()
                {
                    IsActive = true,
                    IsMale = true,
                    Team = -1,
                    AccountID = AccountSQLID,
                    Data = NAPI.Util.ToJson(newCharacter.CurrentAppearance)
                };

                Outfit defaultFugitive = new Outfit("Default Fugitive", "fugitive", true, 0);
                Outfit defaultCop = new Outfit("Default Cop", "cop", true, 1);

                SavedOutfit savedFugitive = new SavedOutfit()
                {
                    IsActive = true,
                    IsMale = newCharacter.CurrentAppearance.IsMale,
                    Team = 0,
                    Data = NAPI.Util.ToJson(defaultFugitive),
                    AccountID = AccountSQLID
                };
                SavedOutfit savedCop = new SavedOutfit()
                {
                    IsActive = true,
                    IsMale = newCharacter.CurrentAppearance.IsMale,
                    Team = 1,
                    Data = NAPI.Util.ToJson(defaultCop),
                    AccountID = AccountSQLID
                };


                List<SavedAppearance> appearances = new List<SavedAppearance>() { firstAppearance };
                List<SavedOutfit> outfits = new List<SavedOutfit>() { savedCop, savedFugitive };

                Task.Run(async () =>
                {
                    try
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            await context.SavedAppearances.AddAsync(firstAppearance);
                            await context.SavedOutfits.AddAsync(savedCop);
                            await context.SavedOutfits.AddAsync(savedFugitive);

                            await context.SaveChangesAsync();

                            CharacterSlot character = new CharacterSlot()
                            {
                                AccountID = AccountSQLID,
                                Outfits = NAPI.Util.ToJson(new List<int> { savedCop.SQL_ID, savedFugitive.SQL_ID }),
                                Appearances = NAPI.Util.ToJson(new List<int> { firstAppearance.SQL_ID })
                            };

                            await context.CharacterSlots.AddAsync(character);

                            await context.SaveChangesAsync();

                            NAPI.Util.ConsoleOutput($"[CHARACTER] Added entries for {acc.Username}: [Appearance:] {firstAppearance.SQL_ID} | [Police Outfit:] {savedCop.SQL_ID} | [Fugitive Outfit:] {savedFugitive.SQL_ID}");

                            NAPI.Task.Run(() =>
                            {
                                UpdateActiveOutfitVariable(player, savedFugitive, 0);
                                UpdateActiveOutfitVariable(player, savedCop, 1);

                                SetCharacter(player, firstAppearance, savedCop);

                                ApplyAppearance(player, firstAppearance);
                                ApplyOutfit(player, newCharacter.CurrentAppearance.IsMale, 1, savedCop);
                                player.SetData<bool>("CHARACTER:IsNew", true);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput("Inside try catch char error: " + ex.ToString());
                    }
                });
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput(ex.ToString());
            }
        }

        public static void OpenAppearanceEditor(Player player, Appearance appearance)
        {
            player.SetData("CHARACTER:EditorOpen", true);
            player.SetSharedData("APPEARANCE:Editing", NAPI.Util.ToJson(appearance));

            player.TriggerEvent("CharacterEditor:ToggleAppearanceEditor", true, appearance.IsMale, NAPI.Util.ToJson(appearance));
            SendPlayerToEditorScene(player, -1, false);
        }
        public static void OpenAppearanceEditor(Player player, SavedAppearance appearance)
        {
            Appearance _a = NAPI.Util.FromJson<Appearance>(appearance.Data);
            OpenAppearanceEditor(player, _a);
        }

        public static void OpenOutfitEditor(Player player, SavedOutfit outfit)
        {
            ApplyOutfitData(player, NAPI.Util.FromJson<Outfit>(outfit.Data));

            player.SetData("CHARACTER:EditorOpen", true);

            Outfit _o = NAPI.Util.FromJson<Outfit>(outfit.Data);
            player.SetSharedData("OUTFIT:Editing", NAPI.Util.ToJson(_o));
            player.SetData<int>("OUTFIT:EditingID", outfit.SQL_ID);

            PropData propData = _o.Props.FirstOrDefault(p => p.Index == (int)PropData.Prop.Bracelet);
            if(propData != null)
                player.TriggerEvent("OutfitCreator:OpenEditor", propData.Drawable, propData.Texture, outfit.IsMale,outfit.Team);
            else player.TriggerEvent("OutfitCreator:OpenEditor", -1, 0, outfit.IsMale, outfit.Team);

            SendPlayerToEditorScene(player, outfit.Team, true);
        }
        public static void SendPlayerToEditorScene(Player player, int team, bool isOutfitEditor)
        {
            player.Dimension = Convert.ToUInt32(player.Id);
            if (!isOutfitEditor)
            {
                player.Position = new Vector3(402.76282f, -996.4929f, -99.00027f);
                player.Heading = 180;
                player.TriggerEvent("CharacterEditor:SceneSetup", 402.731, -997.310, -98.357, 0, 0, 0);
                return;
            }

            if(team == 0)
            {
                player.Position = new Vector3(403.2427f, -996.3667f, -99.00027f);
                player.Heading = 180;
                player.TriggerEvent("CharacterEditor:SceneSetup", 403.731, -1000.14, -98.357, -9, 0, 0);
            }
            else
            {
                player.Position = new Vector3(401.8617f, -1005.00226f, -99.00403f);
                player.Heading = -4;
                player.TriggerEvent("CharacterEditor:SceneSetup", 401.4128, -1000.874, -97.888069, -9, 0, 180);
            }
        }

        public async static Task<SavedAppearance> SaveAppearanceChanges(Account account, Appearance appearance)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                SavedAppearance savedAppearance = await context.SavedAppearances.AsQueryable().FirstOrDefaultAsync(a => a.IsActive == true && a.AccountID == account.SQL_ID);
                if (savedAppearance == null) return null;

                savedAppearance.Data = NAPI.Util.ToJson(appearance);
                await context.SaveChangesAsync();
                return savedAppearance;
            }
        }

        public async static Task<SavedOutfit> SaveOutfitChanges(Account account, int outfitSQLID, Outfit outfit)
        {
            using(MainDatabase context = ContextFactory.CreateContext())
            {
                SavedOutfit savedOutfit = await context.SavedOutfits.AsQueryable().FirstOrDefaultAsync(o => o.AccountID == account.SQL_ID && o.SQL_ID == outfitSQLID);
                if (savedOutfit == null) return null;

                savedOutfit.Data = NAPI.Util.ToJson(outfit);
                await context.SaveChangesAsync();
                return savedOutfit;
            }
        }

        public async static Task<List<SavedOutfit>> GetOutfits(Account account)
        {
            using(MainDatabase context = ContextFactory.CreateContext())
            {
                CharacterSlot character = await context.CharacterSlots.AsQueryable().FirstOrDefaultAsync(c => c.AccountID == account.SQL_ID);
                if (character == null) return null;

                string outfitJSON = character.Outfits;
                List<int> outfitIDs = NAPI.Util.FromJson<List<int>>(outfitJSON);

                List<SavedOutfit> outfits = await context.SavedOutfits.AsQueryable().Where(a => outfitIDs.Contains(a.SQL_ID)).ToListAsync();

                return outfits;
            }
        }
        public async static Task<List<SavedAppearance>> GetAppearances(Account account)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                CharacterSlot character = await context.CharacterSlots.AsQueryable().FirstOrDefaultAsync(c => c.AccountID == account.SQL_ID);
                if (character == null) return null;

                string appearanceJSON = character.Appearances;
                List<int> appearanceIDs = NAPI.Util.FromJson<List<int>>(appearanceJSON);

                List<SavedAppearance> appearances = await context.SavedAppearances.AsQueryable().Where(a => appearanceIDs.Contains(a.SQL_ID)).ToListAsync();

                return appearances;
            }
        }

        public static void SetTeamOutfit(Player player, int teamID)
        {
            SavedOutfit outfit = GetOutfitByTeam(player, teamID);
            if (outfit == null)
            {
                EPlayer.Error(player, "<strong>FATAL ERROR:</strong> Could not find active outfit for team " + teamID);
                // Will apply default outfit if outfit is null
            }
            ApplyOutfit(player, outfit.IsMale, teamID, outfit);
        }

        public static SavedOutfit GetActiveOutfit(Player player, int team)
        {
            if (team == 0 && player.HasSharedData("CHARACTER:FugitiveOutfit")) return NAPI.Util.FromJson<SavedOutfit>(player.GetSharedData<string>("CHARACTER:FugitiveOutfit"));
            if (team == 1 && player.HasSharedData("CHARACTER:PoliceOutfit")) return NAPI.Util.FromJson<SavedOutfit>(player.GetSharedData<string>("CHARACTER:PoliceOutfit"));
            return null;
        }
        public async static Task UpdateActiveOutfitVariables(Player player, Account account)
        {
            List<SavedOutfit> outfits = await GetOutfits(account);
            if (outfits == null || outfits.Count == 0) return;

            SavedOutfit activeCopOutfit = outfits.FirstOrDefault(o => o.Team == 1 && o.IsActive == true);
            SavedOutfit activeFugitiveOutfit = outfits.FirstOrDefault(o => o.Team == 0 && o.IsActive == true);

            if (activeCopOutfit == null || activeFugitiveOutfit == null) return;

            NAPI.Task.Run(() =>
            {
                player.SetSharedData("CHARACTER:PoliceOutfit", NAPI.Util.ToJson(activeCopOutfit));
                player.SetSharedData("CHARACTER:FugitiveOutfit", NAPI.Util.ToJson(activeFugitiveOutfit));
            });
            await NAPI.Task.WaitForMainThread();
        }
        public static void UpdateActiveOutfitVariable(Player player, SavedOutfit outfit, int teamID)
        {
            if(teamID == 0)
            {
                player.SetSharedData("CHARACTER:FugitiveOutfit", NAPI.Util.ToJson(outfit));
            }
            else
            {
                player.SetSharedData("CHARACTER:PoliceOutfit", NAPI.Util.ToJson(outfit));
            }
        }
        

        public async static Task<SavedOutfit> SetActiveOutfit(Player player, Account account, int outfitID, int teamID)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                SavedOutfit _old = await context.SavedOutfits.AsQueryable().FirstOrDefaultAsync(o => o.AccountID == account.SQL_ID && o.Team == teamID && o.IsActive == true);
                SavedOutfit _new = await context.SavedOutfits.AsQueryable().FirstOrDefaultAsync(o => o.SQL_ID == outfitID && o.Team == teamID && o.IsActive == false);
                if (_new == null) return null;

                if (_old != null)
                    _old.IsActive = false;

                _new.IsActive = true;


                await context.SaveChangesAsync();
                NAPI.Task.Run(() =>
                {
                    UpdateActiveOutfitVariable(player, _new, teamID);
                });
                await NAPI.Task.WaitForMainThread();
                return _new;
            }
        }

        public static void SetCharacter(Player player, SavedAppearance appearance, SavedOutfit outfit)
        {
            if (player == null) return;

            Character character = new Character(player, appearance, outfit);
        }

        public static Character GetCharacter(Player player)
        {
            if (player == null) return null;
            if (!player.HasData("CHARACTER:Character")) return new Character(player);

            return player.GetData<Character>("CHARACTER:Character");
        }
        public static Character ResetCharacter(Player player)
        {
            if (player == null) return null;

            return new Character(player, null, null);
        }

        public async static Task<SavedOutfit> CreateOutfit(Account account, int outfitTeam, bool isMale, Outfit outfitInfo)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                SavedOutfit outfit = new SavedOutfit()
                {
                    AccountID = account.SQL_ID,
                    Data = NAPI.Util.ToJson(outfitInfo),
                    Team = outfitTeam,
                    IsActive = true,
                    IsMale = isMale,
                };

                await context.SavedOutfits.AddAsync(outfit);
                await context.SaveChangesAsync();

                CharacterSlot slot = await context.CharacterSlots.AsQueryable().FirstOrDefaultAsync(s => s.AccountID == account.SQL_ID);
                if (slot != null)
                {
                    List<int> outfitIDs = NAPI.Util.FromJson<List<int>>(slot.Outfits);
                    outfitIDs.Add(outfit.SQL_ID);
                    slot.Outfits = NAPI.Util.ToJson(outfitIDs);
                }

                await context.SaveChangesAsync();

                return outfit;
            }
        }
        public static Outfit ApplyOutfit(Player player, bool isMale, int team, SavedOutfit outfit = null, bool update_char = true)
        {
            if (player.HasData("CHARACTER:OutfitOverride"))
            {
                SavedOutfit fit = player.GetData<SavedOutfit>("CHARACTER:OutfitOverride");
                Outfit overrideOutfit = NAPI.Util.FromJson<Outfit>(fit.Data);
                ApplyOutfitData(player, overrideOutfit);
                return overrideOutfit;
            }

            if (outfit == null)
            {
                Outfit _newOutfit = new Outfit("Default", "null", isMale, team);
                ApplyOutfitData(player, _newOutfit);
                return _newOutfit;
            }

            if(update_char)
                SetCharacter(player, GetCharacter(player).SavedAppearance, outfit);

            Outfit _o = NAPI.Util.FromJson<Outfit>(outfit.Data);
            ApplyOutfitData(player, _o);
            return _o;
        }
        public static Appearance ApplyAppearance(Player player, SavedAppearance appearance = null, bool update_char = true)
        {
            if (appearance == null)
            {
                Appearance _newAppearance = new Appearance("Default");
                ApplyAppearanceData(player, _newAppearance);
                return _newAppearance;
            }

            if(update_char)
                SetCharacter(player, appearance, GetCharacter(player).SavedOutfit);

            Appearance _a = NAPI.Util.FromJson<Appearance>(appearance.Data);
            ApplyAppearanceData(player, _a);
            return _a;
        }

        private static void ApplyAppearanceData(Player player, Appearance appearance)
        {
            player.SetSharedData("CHARACTER:Appearance", NAPI.Util.ToJson(appearance));
        }
        private static void ApplyOutfitData(Player player, Outfit outfit)
        {
            player.SetSharedData("CHARACTER:Outfit", NAPI.Util.ToJson(outfit));
        }

        public static SavedOutfit GetOutfitByTeam(Player player, int team)
        {
            if(team == 0)
            {
                if (!player.HasSharedData("CHARACTER:FugitiveOutfit")) return null;
                return NAPI.Util.FromJson<SavedOutfit>(player.GetSharedData<string>("CHARACTER:FugitiveOutfit"));
            }
            if(team == 1)
            {
                if (!player.HasSharedData("CHARACTER:PoliceOutfit")) return null;
                return NAPI.Util.FromJson<SavedOutfit>(player.GetSharedData<string>("CHARACTER:PoliceOutfit"));
            }
            return null;
        }
    }
}
