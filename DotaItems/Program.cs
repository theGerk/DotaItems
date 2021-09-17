using AsyncThen;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DotaItems
{
	static class LinqExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
		{
			foreach (var item in list)
			{
				action(item);
			}
		}

		public static T[] ToArray<T>(this IEnumerable<T> list, int length)
		{
			var output = new T[length];
			int i = 0;
			foreach (var item in list)
			{
				try
				{
					output[i++] = item;
				}
				catch (IndexOutOfRangeException e)
				{
					throw new Exception($"Length of {length} is to small");
				}
			}
			return output;
		}
	}

	static class MiscExtensions
	{
		public static bool TryGetAs<Key, Value, T>(this IDictionary<Key, Value> dict, Key key, out T ret)
		{
			var response = dict.TryGetValue(key, out Value value);
			try
			{
				ret = (T)(object)value;
			}
			catch
			{
				ret = default;
				return false;
			}
			return response;
		}
	}

	class Program
	{
		public readonly static string[] items = new string[]
		{
			"Aghanim's Shard","Bottle","Clarity","Dust of Appearance","Enchanted Mango","Faerie Fire","Healing Salve","Observer Ward","Sentry Ward","Smoke of Deceit","Tango","Tome of Knowledge","Town Portal Scroll","Band of Elvenskin","Belt of Strength","Blade of Alacrity","Circlet","Crown","Gauntlets of Strength","Iron Branch","Mantle of Intelligence","Ogre Axe","Robe of the Magi","Slippers of Agility","Staff of Wizardry","Blades of Attack","Blight Stone","Broadsword","Chainmail","Claymore","Gloves of Haste","Helm of Iron Will","Infused Raindrops","Javelin","Mithril Hammer","Orb of Venom","Quarterstaff","Quelling Blade","Ring of Protection","Blink Dagger","Blitz Knuckles","Boots of Speed","Cloak","Fluffy Hat","Gem of True Sight","Ghost Scepter","Magic Stick","Morbid Mask","Ring of Regen","Sage's Mask","Shadow Amulet","Voodoo Mask","Wind Lace","Demon Edge","Eaglesong","Energy Booster","Hyperstone","Mystic Staff","Platemail","Point Booster","Reaver","Ring of Health","Sacred Relic","Talisman of Evasion","Ultimate Orb","Vitality Booster","Void Stone","Aegis of the Immortal","Aghanim's Blessing","Cheese","Refresher Shard","Boots of Travel","Bracer","Falcon Blade","Hand of Midas","Helm of the Dominator","Helm of the Overlord","Magic Wand","Mask of Madness","Moon Shard","Null Talisman","Oblivion Staff","Orb of Corrosion","Perseverance","Phase Boots","Power Treads","Soul Ring","Wraith Band","Arcane Boots","Buckler","Drum of Endurance","Guardian Greaves","Headdress","Holy Locket","Medallion of Courage","Mekansm","Pipe of Insight","Ring of Basilius","Spirit Vessel","Tranquil Boots","Urn of Shadows","Vladmir's Offering","Aether Lens","Aghanim's Scepter","Dagon","Eul's Scepter of Divinity","Force Staff","Gleipnir","Glimmer Cape","Octarine Core","Refresher Orb","Rod of Atos","Scythe of Vyse","Solar Crest","Veil of Discord","Wind Waker","Witch Blade","Aeon Disk","Assault Cuirass","Black King Bar","Blade Mail","Bloodstone","Crimson Guard","Eternal Shroud","Heart of Tarrasque","Hood of Defiance","Hurricane Pike","Linken's Sphere","Lotus Orb","Manta Style","Shiva's Guard","Soul Booster","Vanguard","Abyssal Blade","Armlet of Mordiggian","Battle Fury","Bloodthorn","Butterfly","Crystalys","Daedalus","Desolator","Divine Rapier","Ethereal Blade","Meteor Hammer","Monkey King Bar","Nullifier","Radiance","Shadow Blade","Silver Edge","Skull Basher","Arcane Blink","Diffusal Blade","Dragon Lance","Echo Sabre","Eye of Skadi","Heaven's Halberd","Kaya","Kaya and Sange","Maelstrom","Mage Slayer","Mjollnir","Overwhelming Blink","Sange","Sange and Yasha","Satanic","Swift Blink","Yasha","Yasha and Kaya","Arcane Ring","Broom Handle","Chipped Vest","Fairy's Trinket","Keen Optic","Ocean Heart","Pig Pole","Possessed Mask","Trusty Shovel","Tumbler's Toy","Brigand's Blade","Bullwhip","Dragon Scale","Essence Ring","Fae Grenade","Grove Bow","Nether Shawl","Philosopher's Stone","Pupil's Gift","Quicksilver Amulet","Ring of Aquila","Vambrace","Blast Rig","Ceremonial Robe","Cloak of Flames","Elven Tunic","Enchanted Quiver","Mind Breaker","Paladin Sword","Psychic Headband","Quickening Charm","Spider Legs","Telescope","Titan Sliver","Ascetic's Cap","Flicker","Ninja Gear","Penta-Edged Sword","Spell Prism","Stormcrafter","The Leveller","Timeless Relic","Trickster Cloak","Witchbane","Apex","Arcanist's Armor","Book of Shadows","Book of the Dead","Ex Machina","Fallen Sky","Force Boots","Giant's Ring","Mirror Shield","Pirate Hat","Seer Stone","Stygian Desolator","Ballista","Clumsy Net","Craggy Coat","Elixir","Faded Broach","Fusion Rune","Greater Faerie Fire","Havoc Hammer","Helm of the Undying","Illusionist's Cape","Imp Claw","Iron Talon","Ironwood Tree","Magic Lamp","Mango Tree","Minotaur Horn","Necronomicon","Orb of Destruction","Phoenix Ash","Poor Man's Shield","Prince's Knife","Repair Kit","Ring of Tarrasque","Royal Jelly","Stout Shield","Third Eye","Tome of Aghanim","Trident","Vampire Fangs","Witless Shako","Woodland Striders"
		};

		static string getItemUrl(string itemName)
		{
			var formatedItem = HttpUtility.UrlEncode(itemName.Replace(' ', '_'));
			return $"https://liquipedia.net/dota2/index.php?title={formatedItem}&action=edit&section=0";
		}

		static async Task<string> ReadPage(string url)
		{
			var web = new HtmlWeb();
			var loaded = await web.LoadFromWebAsync(url);
			var elem = loaded.GetElementbyId("wpTextbox1");
			return elem.InnerText;
		}

		static Dictionary<string, object> Consume(string semiUsable)
		{
			return new WikiParser(semiUsable).ReadObject();
		}

		const string ITEM_DATA_FILE = @"../../../itemdata.txt";

		static void Main(string[] args)
		{
			string[] semiUsable;
			if (!File.Exists(ITEM_DATA_FILE))
			{
				semiUsable = items
					.Select(getItemUrl)
					.Select(x => ReadPage(x).Result)
					.ToArray(items.Length);

				File.WriteAllText(
					ITEM_DATA_FILE,
					semiUsable
						.Select((x, i) => $"--{items[i]}:\n{x}")
						.Aggregate((a, b) => $"{a}\n{b}")
						+ "\n"
				);
			}
			else
			{
				semiUsable = File.ReadAllLines(ITEM_DATA_FILE)
					.Select(x => x.StartsWith("--") ? "--" : x)
					.Aggregate((a, b) => $"{a}\n{b}")
					.Split("--\n")
					.Skip(1)
					.Select(x => x[0..^1])
					.ToArray(items.Length);
			}


			var usable = semiUsable.Select(Consume);

			usable.Select(x => x.TryGetAs("active", out Dictionary<string, object> active) ? active.Keys.Except(new string[] { "ability", "color", "AbilityIcon", "abilityicon" }).FirstOrDefault() : null).ForEach(Console.WriteLine);
			//Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(usable));
			File.WriteAllText("../../../items.json", Newtonsoft.Json.JsonConvert.SerializeObject(usable));
		}
	}
}
