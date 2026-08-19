# One-shot generator for campaign/gamein.xml (Windows-1251 ASCII). Run then discard.
from pathlib import Path
import random

random.seed(42)

OUT = Path(__file__).with_name("gamein.xml")

DUR = {
    "grassl": 2, "dust": 3, "barren": 3, "mountn": 5, "sea": 6, "ocean": 7,
    "smmast": 3, "smcast": 3, "lrmast": 4, "lrcast": 4, "orbit": 1,
}


class IdPool:
    """Pre-allocates random unique IDs per prefix from a seeded RNG."""

    def __init__(self):
        self._pools: dict[str, list[int]] = {}
        self._idx: dict[str, int] = {}

    def init(self, prefix: str, width: int, count: int, range_max: int | None = None):
        if range_max is None:
            range_max = 10**width - 1
        nums = random.sample(range(1, range_max + 1), count)
        self._pools[prefix] = nums
        self._idx[prefix] = 0

    def next(self, prefix: str, width: int) -> str:
        i = self._idx[prefix]
        self._idx[prefix] = i + 1
        return f"{prefix}{self._pools[prefix][i]:0{width}d}"

    def peek_num(self, prefix: str, index: int) -> int:
        return self._pools[prefix][index]


IDP = IdPool()

# Allocate pools — generous counts
IDP.init("R", 5, 260)        # regions (Arbor 36 + Anvil 35 + Scoria 24 + Pyre 20 + moons 107 + empty ~18)
IDP.init("P", 5, 35)         # planets
IDP.init("M", 5, 12)         # moons
IDP.init("S", 5, 12)         # stars
IDP.init("O", 5, 50)         # orbits
IDP.init("SS", 4, 12)        # systems
IDP.init("A", 5, 20)         # Alderson points
IDP.init("O0A", 3, 20)       # AP orbits
IDP.init("STK", 6, 200, 999999)  # stacks + persons (shared 6-digit namespace)

# Pre-draw all IDs in a deterministic order, storing by semantic key.
# Systems
SYS = {}
for key in ["Helios", "Fomal", "Ember", "Gleam", "Cinder", "Ash", "Shards", "Deep", "Graph", "Spare"]:
    SYS[key] = IDP.next("SS", 4)

# Stars
STAR = {}
for key in ["Helios", "Fomal", "Ember", "Gleam", "Cinder", "Ash", "Shards", "Deep", "Graph", "Spare"]:
    STAR[key] = IDP.next("S", 5)

# Planets (order matters for determinism, not for correctness)
PLN = {}
for key in [
    "Arbor", "Scoria", "HeliosBelt", "Aeolus",
    "Anvil", "Pyre", "FomalBelt", "FomalGiant",
    "EmberDust", "EmberBelt", "EmberGiant",
    "GleamInner", "GleamBelt", "GleamIce",
    "CinderDust", "CinderBelt", "CinderGiant",
    "AshIce", "AshInnerGiant", "AshOuterGiant",
    "ShardsDust", "ShardsInnerBelt", "ShardsOuterBelt",
    "DeepDust", "DeepInnerGiant", "DeepOuterGiant",
    "Graph", "GraphDust", "GraphBelt",
    "SpareDust", "SpareBelt",
]:
    PLN[key] = IDP.next("P", 5)

# Moons
MOON = {}
for key in ["Selene", "Crucible", "Quench", "Boreas", "Zephyr", "Notus", "Eurus", "Nereid", "Tethys"]:
    MOON[key] = IDP.next("M", 5)

# Orbits — one per planet/moon
ORB = {}
for key in [
    "Arbor", "Scoria", "HeliosBelt", "Aeolus",
    "Anvil", "Pyre", "FomalBelt", "FomalGiant",
    "Selene", "Crucible", "Quench", "Boreas", "Zephyr", "Notus", "Eurus", "Nereid", "Tethys",
    "EmberDust", "EmberBelt", "EmberGiant",
    "GleamInner", "GleamBelt", "GleamIce",
    "CinderDust", "CinderBelt", "CinderGiant",
    "AshIce", "AshInnerGiant", "AshOuterGiant",
    "ShardsDust", "ShardsInnerBelt", "ShardsOuterBelt",
    "DeepDust", "DeepInnerGiant", "DeepOuterGiant",
    "Graph", "GraphDust", "GraphBelt",
    "SpareDust", "SpareBelt",
]:
    ORB[key] = IDP.next("O", 5)

# Alderson points — 18, pre-draw in order
AP_KEYS = [
    "HelCin", "CinHel", "CinEmb", "EmbCin", "CinGle", "GleCin",
    "CinAsh", "AshCin", "FomSha", "ShaFom", "ShaDee", "DeeSha",
    "ShaGra", "GraSha", "ShaSpa", "SpaSha", "CinShaU", "ShaCinU",
]
AP_ID = {}
AP_ORB = {}
for key in AP_KEYS:
    AP_ID[key] = IDP.next("A", 5)
    AP_ORB[key] = IDP.next("O0A", 3)

# Regions — draw in deterministic order
REG = {}

# Arbor: 36 regions, keyed as ("arbor", x, y)
for y in range(6):
    for x in range(6):
        REG[("arbor", x, y)] = IDP.next("R", 5)

# Anvil: 35 regions
for y in range(5):
    for x in range(7):
        REG[("anvil", x, y)] = IDP.next("R", 5)

# Scoria: 24 regions (6×4)
for y in range(4):
    for x in range(6):
        REG[("scoria", x, y)] = IDP.next("R", 5)

# Pyre: 20 regions (5×4)
for y in range(4):
    for x in range(5):
        REG[("pyre", x, y)] = IDP.next("R", 5)

# Moons
MOON_SPECS = {
    "selene": (5, 3), "boreas": (5, 2), "zephyr": (5, 2),
    "notus": (5, 3), "eurus": (4, 3), "crucible": (5, 3),
    "quench": (5, 2), "nereid": (5, 2), "tethys": (5, 2),
}
for mname, (w, h) in MOON_SPECS.items():
    for y in range(h):
        for x in range(w):
            REG[(mname, x, y)] = IDP.next("R", 5)

# Empty-system landing regions
EMPTY_REG_KEYS = [
    "EmberBrine", "EmberRock",
    "GleamCrust", "GleamMetal", "GleamIceLand",
    "CinderFumarole", "CinderCarbon",
    "AshXenon",
    "ShardsDustLand", "ShardsOrganics", "ShardsIce",
    "DeepMethane",
    "GraphPrairie", "GraphHighland", "GraphRock",
    "SpareBeryllium", "SpareIce",
]
for key in EMPTY_REG_KEYS:
    REG[("empty", key)] = IDP.next("R", 5)

# Stacks — UN cities + HQs. Draw in order.
STK = {}

# UN cities: Assembly, Tidewatch, Windgap, Slagport, Ridge, Isotope
# Each city needs: city stack, garrison, farms, energy1, [energy2], granary = up to 7 sub-stacks
for city in ["Assembly", "Tidewatch", "Windgap", "Slagport", "Ridge", "Isotope"]:
    for sub in ["city", "garrison", "farms", "cplant", "wnplnt", "granary"]:
        STK[(city, sub)] = IDP.next("STK", 6)

# HQs for factions 2-11
# Each HQ: hq, person, cargo, farms, cdrill, factry, energy
for fac in range(2, 12):
    for sub in ["hq", "person", "cargo", "farms", "cdrill", "factry", "energy"]:
        STK[("hq", fac, sub)] = IDP.next("STK", 6)


def esc(s):
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;").replace('"', "&quot;")


def resources_xml(res, indent):
    lines = []
    for t, q in res:
        lines.append(f'{indent}<resource type="{t}" quantity="{q}" />')
    return "\n".join(lines)


def capacities_xml(caps, indent):
    lines = []
    for g, q in caps:
        lines.append(f'{indent}<capacity group="{g}" quantity="{q}" />')
    return "\n".join(lines)


def exits_xml(exits, indent):
    lines = []
    for target, mode, dur in exits:
        attr = "orbit" if mode == "orbit" else "region"
        lines.append(f'{indent}<exit {attr}="{target}">')
        lines.append(f'{indent}\t<exitmode mode="{mode}" duration="{dur}" />')
        lines.append(f"{indent}</exit>")
    return "\n".join(lines)


def item(typ, qty, indent):
    return f'{indent}<itemstack type="{typ}" quantity="{qty}" />'


def nest_upkeep(cash, indent, food=0):
    lines = []
    if food:
        lines.append(f'{indent}<upkeep type="food" quantity="{food}" />')
    lines.append(f'{indent}<upkeep type="cash" quantity="{cash}" />')
    return "\n".join(lines)


# (x, y, type, name-en, occupant, resources)
# occupant: None | ("sp",) | ("un", name, city_qty) | ("hq", fac)
ARBOR = [
    (0, 0, "ocean", "West Pelagic", None, [("terair", 100), ("water", 600), ("food", 40)]),
    (1, 0, "sea", "Shelf", None, [("terair", 100), ("water", 400), ("food", 80), ("h2o2", 100)]),
    (2, 0, "grassl", "Tidewatch Coast", ("un", "Tidewatch", 2), [("terair", 100), ("food", 500), ("oil", 30), ("iron", 15), ("water", 150)]),
    (3, 0, "grassl", "South Vale", None, [("terair", 100), ("food", 450), ("carbon", 25), ("iron", 20), ("water", 120)]),
    (4, 0, "dust", "Launch Steppe", None, [("iron", 30), ("silici", 25), ("carbon", 10), ("titani", 5)]),
    (5, 0, "barren", "Cinder Flats", None, [("iron", 20), ("silici", 15)]),
    (0, 1, "ocean", "West Deep", None, [("terair", 100), ("water", 600), ("food", 40)]),
    (1, 1, "grassl", "Northwind Grant", ("hq", 2), [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    (2, 1, "grassl", "Mid Vale", None, [("terair", 100), ("food", 500), ("carbon", 20), ("iron", 15), ("water", 140)]),
    (3, 1, "grassl", "Greenwell Grant", ("hq", 3), [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    (4, 1, "mountn", "South Ridge", None, [("iron", 70), ("silici", 20), ("carbon", 5), ("titani", 3)]),
    (5, 1, "dust", "East Dune", None, [("iron", 30), ("silici", 25), ("carbon", 10)]),
    (0, 2, "sea", "West Coast", None, [("terair", 100), ("water", 400), ("food", 80), ("h2o2", 80)]),
    (1, 2, "grassl", "Farm Belt", None, [("terair", 100), ("food", 700), ("carbon", 35), ("iron", 20), ("water", 160)]),
    (2, 2, "grassl", "Assembly Basin", ("un", "Assembly", 6), [("terair", 100), ("food", 800), ("carbon", 40), ("iron", 25), ("water", 180)]),
    (3, 2, "grassl", "Central Basin", None, [("terair", 100), ("food", 550), ("carbon", 25), ("iron", 20), ("water", 150)]),
    (4, 2, "grassl", "Rivermark Grant", ("hq", 4), [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    (5, 2, "mountn", "East Peak", None, [("iron", 80), ("silici", 25)]),
    (0, 3, "ocean", "Mid Pelagic", None, [("terair", 100), ("water", 600), ("food", 30)]),
    (1, 3, "ocean", "Inner Pelagic", None, [("terair", 100), ("water", 600), ("food", 30)]),
    (2, 3, "grassl", "Prairie", None, [("terair", 100), ("food", 500), ("carbon", 20), ("iron", 15), ("water", 140)]),
    (3, 3, "grassl", "Sundock Grant", ("hq", 5), [("terair", 100), ("food", 600), ("oil", 20), ("iron", 20), ("water", 150)]),
    (4, 3, "grassl", "East Steppe", None, [("terair", 100), ("food", 480), ("carbon", 20), ("iron", 18), ("water", 130)]),
    (5, 3, "mountn", "East Crag", None, [("iron", 75), ("silici", 20)]),
    (0, 4, "ocean", "North Pelagic", None, [("terair", 100), ("water", 600), ("food", 30)]),
    (1, 4, "sea", "North Sound", None, [("terair", 100), ("water", 400), ("food", 70), ("h2o2", 90)]),
    (2, 4, "grassl", "Copse Grant", ("hq", 6), [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    (3, 4, "grassl", "Windgap", ("un", "Windgap", 1), [("terair", 100), ("food", 450), ("carbon", 20), ("iron", 15), ("water", 140)]),
    (4, 4, "dust", "Loess", None, [("iron", 35), ("silici", 20), ("carbon", 15)]),
    (5, 4, "barren", "East Flat", None, [("iron", 20), ("silici", 15)]),
    (0, 5, "ocean", "Polar Ocean", None, [("terair", 100), ("water", 700), ("food", 20)]),
    (1, 5, "ocean", "Polar Ocean E", None, [("terair", 100), ("water", 700), ("food", 20)]),
    (2, 5, "sea", "Polar Sea", None, [("terair", 100), ("water", 500), ("food", 40), ("h2o2", 120)]),
    (3, 5, "grassl", "Tundra", None, [("terair", 100), ("food", 250), ("carbon", 10), ("iron", 10), ("water", 200)]),
    (4, 5, "mountn", "North Spine", None, [("iron", 60), ("silici", 20)]),
    (5, 5, "dust", "Polar Dust", None, [("iron", 25), ("silici", 20)]),
]

ANVIL = [
    (0, 0, "ocean", "West Sea", None, [("terair", 100), ("water", 350), ("food", 10)]),
    (1, 0, "sea", "West Shelf", None, [("terair", 100), ("water", 200), ("food", 30)]),
    (2, 0, "dust", "Copper Flat", None, [("copper", 25), ("silici", 40), ("titani", 20), ("iron", 25)]),
    (3, 0, "grassl", "Slagport", ("un", "Slagport", 2), [("terair", 100), ("food", 120), ("water", 80), ("iron", 20), ("silici", 30), ("titani", 20)]),
    (4, 0, "mountn", "South Ore", None, [("titani", 60), ("silici", 40), ("copper", 30), ("iron", 30)]),
    (5, 0, "dust", "South Dune", None, [("copper", 20), ("silici", 40), ("titani", 15), ("iron", 25)]),
    (6, 0, "barren", "South Scarp", None, [("silici", 50), ("titani", 20)]),
    (0, 1, "sea", "Northwest Sea", None, [("terair", 100), ("water", 200), ("food", 25)]),
    (1, 1, "grassl", "Ironclad Grant", ("hq", 7), [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("titani", 25), ("silici", 30)]),
    (2, 1, "grassl", "Slope", None, [("terair", 100), ("food", 100), ("water", 70), ("silici", 25), ("titani", 15)]),
    (3, 1, "mountn", "West Spine", None, [("titani", 70), ("copper", 35), ("silici", 40), ("iron", 30)]),
    (4, 1, "mountn", "Mid Spine", None, [("titani", 50), ("uraniu", 80), ("silici", 30), ("copper", 20)]),
    (5, 1, "grassl", "Oreline Grant", ("hq", 8), [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("titani", 25), ("silici", 30)]),
    (6, 1, "dust", "East Talus", None, [("copper", 25), ("silici", 40), ("titani", 20)]),
    (0, 2, "grassl", "Marsh", None, [("terair", 100), ("food", 150), ("water", 100), ("iron", 15), ("silici", 20)]),
    (1, 2, "grassl", "Bench", None, [("terair", 100), ("food", 110), ("water", 70), ("silici", 25), ("titani", 15)]),
    (2, 2, "mountn", "Crag", None, [("titani", 80), ("copper", 40), ("silici", 45), ("iron", 35)]),
    (3, 2, "grassl", "Basalt Grant", ("hq", 9), [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("copper", 20), ("silici", 30)]),
    (4, 2, "dust", "Scree", None, [("copper", 30), ("silici", 40), ("titani", 25), ("iron", 20)]),
    (5, 2, "mountn", "East Peak", None, [("titani", 55), ("uraniu", 120), ("silici", 35), ("copper", 25)]),
    (6, 2, "grassl", "Ridge", ("un", "Ridge", 1), [("terair", 100), ("food", 100), ("water", 70), ("silici", 30), ("titani", 20), ("iron", 15)]),
    (0, 3, "ocean", "Gulf", None, [("terair", 100), ("water", 350), ("food", 10)]),
    (1, 3, "grassl", "Silicate Grant", ("hq", 10), [("terair", 100), ("food", 140), ("water", 80), ("silici", 40), ("titani", 20), ("iron", 15)]),
    (2, 3, "grassl", "Vale", None, [("terair", 100), ("food", 90), ("water", 70), ("silici", 25), ("iron", 15)]),
    (3, 3, "mountn", "Uraninite", None, [("uraniu", 150), ("titani", 40), ("silici", 30), ("copper", 20)]),
    (4, 3, "grassl", "Thin Soil", None, [("terair", 100), ("food", 80), ("water", 60), ("silici", 20), ("titani", 10)]),
    (5, 3, "grassl", "Fission Grant", ("hq", 11), [("terair", 100), ("food", 140), ("water", 80), ("uraniu", 15), ("titani", 20), ("silici", 30)]),
    (6, 3, "dust", "Fan", None, [("copper", 20), ("silici", 45), ("titani", 15)]),
    (0, 4, "ocean", "North Sea", None, [("terair", 100), ("water", 350), ("food", 8)]),
    (1, 4, "sea", "North Shelf", None, [("terair", 100), ("water", 200), ("food", 20)]),
    (2, 4, "dust", "Ash", None, [("copper", 15), ("silici", 35), ("titani", 15), ("iron", 20)]),
    (3, 4, "grassl", "Isotope", ("un", "Isotope", 1), [("terair", 100), ("food", 90), ("water", 70), ("silici", 25), ("titani", 15), ("iron", 15)]),
    (4, 4, "mountn", "Shield", None, [("titani", 65), ("silici", 40), ("copper", 30), ("iron", 30)]),
    (5, 4, "barren", "Glass", None, [("silici", 50), ("titani", 15)]),
    (6, 4, "dust", "North Reg", None, [("copper", 20), ("silici", 40), ("iron", 20)]),
]

SCORIA = [
    (0,0,"dust","Scoria West",[("titani",50),("copper",30),("silici",30),("iron",25)]),
    (1,0,"dust","Scoria Flats",[("titani",60),("copper",40),("silici",40),("iron",30)]),
    (2,0,"mountn","Scoria Ridge",[("titani",80),("copper",50),("silici",35),("iron",40)]),
    (3,0,"dust","Scoria Central",[("titani",55),("copper",35),("silici",30),("iron",25)]),
    (4,0,"barren","Scoria East",[("silici",20),("iron",15)]),
    (5,0,"dust","Scoria Crater",[("titani",45),("copper",25),("silici",25),("iron",20)]),
    (0,1,"barren","Scoria Polar S",[("silici",15),("iron",10)]),
    (1,1,"dust","Scoria Basin",[("titani",70),("copper",45),("silici",35),("iron",35)]),
    (2,1,"mountn","Scoria Peak",[("titani",90),("copper",60),("silici",40),("iron",50)]),
    (3,1,"dust","Scoria Rill",[("titani",50),("copper",30),("silici",25),("iron",20)]),
    (4,1,"dust","Scoria Dune",[("titani",40),("copper",20),("silici",25),("iron",20)]),
    (5,1,"barren","Scoria Rim E",[("silici",20),("iron",10)]),
    (0,2,"dust","Scoria Trench",[("titani",55),("copper",35),("silici",30),("iron",25)]),
    (1,2,"mountn","Scoria Vein",[("titani",85),("copper",55),("silici",40),("iron",45)]),
    (2,2,"dust","Scoria Plain",[("titani",60),("copper",40),("silici",30),("iron",30)]),
    (3,2,"barren","Scoria Waste",[("silici",20),("iron",15)]),
    (4,2,"dust","Scoria Fan",[("titani",45),("copper",25),("silici",25),("iron",20)]),
    (5,2,"barren","Scoria Rim SE",[("silici",15),("iron",10)]),
    (0,3,"barren","Scoria Polar N",[("silici",15),("iron",10)]),
    (1,3,"dust","Scoria North",[("titani",50),("copper",30),("silici",25),("iron",20)]),
    (2,3,"dust","Scoria Shelf",[("titani",55),("copper",35),("silici",30),("iron",25)]),
    (3,3,"dust","Scoria NE",[("titani",40),("copper",20),("silici",20),("iron",15)]),
    (4,3,"barren","Scoria Far",[("silici",15),("iron",10)]),
    (5,3,"barren","Scoria Polar NE",[("silici",10)]),
]

PYRE = [
    (0,0,"dust","Pyre West",[("iron",50),("silici",35),("copper",15)]),
    (1,0,"dust","Pyre Basin",[("iron",70),("silici",45),("copper",20),("titani",10)]),
    (2,0,"mountn","Pyre Ridge",[("iron",90),("silici",50),("copper",25),("titani",15)]),
    (3,0,"dust","Pyre East",[("iron",55),("silici",35),("copper",15)]),
    (4,0,"barren","Pyre Scarp",[("silici",25),("iron",20)]),
    (0,1,"barren","Pyre Polar S",[("silici",20),("iron",15)]),
    (1,1,"dust","Pyre Crucible",[("iron",80),("silici",50),("copper",25),("titani",12)]),
    (2,1,"mountn","Pyre Core",[("iron",100),("silici",55),("copper",30),("titani",20)]),
    (3,1,"dust","Pyre Vent",[("iron",60),("silici",40),("copper",18)]),
    (4,1,"barren","Pyre Rim",[("silici",20),("iron",15)]),
    (0,2,"dust","Pyre Trench",[("iron",55),("silici",35),("copper",15)]),
    (1,2,"dust","Pyre Flats",[("iron",65),("silici",40),("copper",20)]),
    (2,2,"dust","Pyre Central",[("iron",75),("silici",45),("copper",22),("titani",10)]),
    (3,2,"barren","Pyre Waste",[("silici",25),("iron",20)]),
    (4,2,"barren","Pyre Far E",[("silici",15),("iron",10)]),
    (0,3,"barren","Pyre Polar N",[("silici",15),("iron",10)]),
    (1,3,"dust","Pyre North",[("iron",60),("silici",35),("copper",15)]),
    (2,3,"dust","Pyre Shelf",[("iron",50),("silici",30),("copper",12)]),
    (3,3,"barren","Pyre NE",[("silici",20),("iron",15)]),
    (4,3,"barren","Pyre Polar NE",[("silici",10)]),
]

SELENE_DATA = [
    (0,0,"dust","Selene West",[("titani",40),("silici",30),("iron",15)]),
    (1,0,"dust","Selene Flats",[("titani",50),("silici",25),("iron",10)]),
    (2,0,"mountn","Selene Ridge",[("titani",80),("silici",40),("iron",25)]),
    (3,0,"dust","Selene East",[("titani",35),("silici",30),("iron",15)]),
    (4,0,"barren","Selene Scarp",[("silici",20),("iron",10)]),
    (0,1,"barren","Selene Polar S",[("silici",25),("iron",10)]),
    (1,1,"dust","Selene Basin",[("titani",60),("silici",35),("iron",20)]),
    (2,1,"mountn","Selene Peak",[("titani",70),("silici",45),("iron",30)]),
    (3,1,"dust","Selene Rill",[("titani",45),("silici",30),("iron",15)]),
    (4,1,"barren","Selene Rim",[("silici",20),("iron",10)]),
    (0,2,"barren","Selene Polar N",[("silici",20),("iron",10)]),
    (1,2,"dust","Selene Trench",[("titani",55),("silici",30),("iron",15)]),
    (2,2,"dust","Selene Caldera",[("titani",45),("silici",35),("iron",20)]),
    (3,2,"barren","Selene Waste",[("silici",25),("iron",10)]),
    (4,2,"barren","Selene Far",[("silici",15)]),
]
BOREAS_DATA = [
    (0,0,"dust","Boreas West Ice",[("heliu3",30),("h2o2",50),("water",40)]),
    (1,0,"dust","Boreas Basin",[("heliu3",40),("h2o2",60),("water",50)]),
    (2,0,"barren","Boreas Ridge",[("heliu3",20),("h2o2",30)]),
    (3,0,"dust","Boreas East Ice",[("heliu3",35),("h2o2",45),("water",40)]),
    (4,0,"barren","Boreas Scarp",[("heliu3",15),("h2o2",25)]),
    (0,1,"barren","Boreas South",[("heliu3",15),("h2o2",30)]),
    (1,1,"dust","Boreas Deep Ice",[("heliu3",50),("h2o2",80),("water",60)]),
    (2,1,"dust","Boreas Vent",[("heliu3",45),("h2o2",70),("water",50)]),
    (3,1,"barren","Boreas Polar",[("heliu3",20),("h2o2",35)]),
    (4,1,"barren","Boreas Far",[("heliu3",10),("h2o2",20)]),
]
ZEPHYR_DATA = [
    (0,0,"dust","Zephyr West",[("h2o2",80),("water",60)]),
    (1,0,"dust","Zephyr Basin",[("h2o2",100),("water",80),("heliu3",10)]),
    (2,0,"barren","Zephyr Crest",[("h2o2",40),("water",30)]),
    (3,0,"dust","Zephyr East",[("h2o2",70),("water",50)]),
    (4,0,"barren","Zephyr Rim",[("h2o2",30),("water",20)]),
    (0,1,"barren","Zephyr Polar S",[("h2o2",35),("water",25)]),
    (1,1,"dust","Zephyr Deep",[("h2o2",120),("water",100),("heliu3",15)]),
    (2,1,"dust","Zephyr Vent",[("h2o2",90),("water",70)]),
    (3,1,"barren","Zephyr Shelf",[("h2o2",40),("water",30)]),
    (4,1,"barren","Zephyr Far",[("h2o2",25)]),
]
NOTUS_DATA = [
    (0,0,"dust","Notus West",[("tungst",20),("iron",30),("silici",25)]),
    (1,0,"mountn","Notus Vein",[("tungst",60),("iron",40),("silici",30)]),
    (2,0,"dust","Notus Central",[("tungst",30),("iron",35),("silici",25)]),
    (3,0,"mountn","Notus East Peak",[("tungst",50),("iron",45),("silici",35)]),
    (4,0,"barren","Notus Scarp",[("iron",15),("silici",20)]),
    (0,1,"barren","Notus Polar S",[("iron",10),("silici",15)]),
    (1,1,"dust","Notus Basin",[("tungst",40),("iron",35),("silici",25)]),
    (2,1,"mountn","Notus Core",[("tungst",80),("iron",50),("silici",40)]),
    (3,1,"dust","Notus Rill",[("tungst",25),("iron",30),("silici",20)]),
    (4,1,"barren","Notus Rim",[("iron",15),("silici",15)]),
    (0,2,"barren","Notus Far W",[("iron",10),("silici",10)]),
    (1,2,"dust","Notus Trench",[("tungst",35),("iron",30),("silici",20)]),
    (2,2,"dust","Notus Shelf",[("tungst",25),("iron",25),("silici",20)]),
    (3,2,"barren","Notus Waste",[("iron",15),("silici",15)]),
    (4,2,"barren","Notus Polar N",[("iron",10),("silici",10)]),
]
EURUS_DATA = [
    (0,0,"dust","Eurus Caldera",[("silici",60),("iron",25),("titani",15)]),
    (1,0,"mountn","Eurus Flow",[("silici",80),("iron",40),("titani",25)]),
    (2,0,"dust","Eurus Ash",[("silici",50),("iron",20),("titani",10)]),
    (3,0,"barren","Eurus Scarp",[("silici",30),("iron",15)]),
    (0,1,"barren","Eurus Polar S",[("silici",25),("iron",10)]),
    (1,1,"dust","Eurus Vent",[("silici",70),("iron",35),("titani",20)]),
    (2,1,"mountn","Eurus Spine",[("silici",90),("iron",45),("titani",30)]),
    (3,1,"barren","Eurus Rim",[("silici",25),("iron",10)]),
    (0,2,"barren","Eurus Far W",[("silici",20),("iron",10)]),
    (1,2,"dust","Eurus Plain",[("silici",50),("iron",25),("titani",15)]),
    (2,2,"dust","Eurus Basin",[("silici",45),("iron",20),("titani",10)]),
    (3,2,"barren","Eurus Polar N",[("silici",20),("iron",10)]),
]
CRUCIBLE_DATA = [
    (0,0,"dust","Crucible West",[("iron",40),("copper",25),("titani",20),("silici",15)]),
    (1,0,"mountn","Crucible Lode",[("iron",60),("copper",40),("titani",35),("silici",25)]),
    (2,0,"dust","Crucible Central",[("iron",35),("copper",20),("titani",15),("silici",20)]),
    (3,0,"mountn","Crucible East Vein",[("iron",55),("copper",35),("titani",30),("silici",25)]),
    (4,0,"barren","Crucible Scarp",[("iron",15),("silici",15)]),
    (0,1,"barren","Crucible Polar S",[("iron",10),("silici",10)]),
    (1,1,"dust","Crucible Basin",[("iron",50),("copper",30),("titani",25),("silici",20)]),
    (2,1,"mountn","Crucible Core",[("iron",70),("copper",45),("titani",40),("silici",30)]),
    (3,1,"dust","Crucible Rill",[("iron",40),("copper",25),("titani",20),("silici",15)]),
    (4,1,"barren","Crucible Rim",[("iron",15),("silici",10)]),
    (0,2,"barren","Crucible Far W",[("iron",10),("silici",10)]),
    (1,2,"dust","Crucible Trench",[("iron",45),("copper",30),("titani",20),("silici",15)]),
    (2,2,"dust","Crucible Shelf",[("iron",35),("copper",20),("titani",15),("silici",15)]),
    (3,2,"barren","Crucible Waste",[("iron",15),("silici",10)]),
    (4,2,"barren","Crucible Polar N",[("iron",10),("silici",10)]),
]
QUENCH_DATA = [
    (0,0,"dust","Quench West",[("water",80),("h2o2",50)]),
    (1,0,"dust","Quench Basin",[("water",120),("h2o2",80)]),
    (2,0,"barren","Quench Ridge",[("water",40),("h2o2",30)]),
    (3,0,"dust","Quench East",[("water",90),("h2o2",60)]),
    (4,0,"barren","Quench Rim",[("water",30),("h2o2",20)]),
    (0,1,"barren","Quench Polar S",[("water",30),("h2o2",20)]),
    (1,1,"dust","Quench Deep",[("water",150),("h2o2",100)]),
    (2,1,"dust","Quench Vent",[("water",100),("h2o2",70)]),
    (3,1,"barren","Quench Shelf",[("water",40),("h2o2",25)]),
    (4,1,"barren","Quench Far",[("water",20),("h2o2",15)]),
]
NEREID_DATA = [
    (0,0,"dust","Nereid West",[("heliu3",35),("h2o2",50),("water",40)]),
    (1,0,"dust","Nereid Basin",[("heliu3",45),("h2o2",70),("water",55)]),
    (2,0,"barren","Nereid Crest",[("heliu3",20),("h2o2",30)]),
    (3,0,"dust","Nereid East",[("heliu3",40),("h2o2",55),("water",40)]),
    (4,0,"barren","Nereid Rim",[("heliu3",15),("h2o2",20)]),
    (0,1,"barren","Nereid Polar S",[("heliu3",15),("h2o2",25)]),
    (1,1,"dust","Nereid Deep",[("heliu3",55),("h2o2",80),("water",65)]),
    (2,1,"dust","Nereid Vent",[("heliu3",40),("h2o2",65),("water",50)]),
    (3,1,"barren","Nereid Shelf",[("heliu3",20),("h2o2",30)]),
    (4,1,"barren","Nereid Far",[("heliu3",10),("h2o2",15)]),
]
TETHYS_DATA = [
    (0,0,"dust","Tethys West",[("ammoni",50),("water",40),("methn",10)]),
    (1,0,"dust","Tethys Basin",[("ammoni",70),("water",60),("methn",15)]),
    (2,0,"barren","Tethys Crest",[("ammoni",25),("water",20)]),
    (3,0,"dust","Tethys East",[("ammoni",55),("water",45),("methn",12)]),
    (4,0,"barren","Tethys Rim",[("ammoni",20),("water",15)]),
    (0,1,"barren","Tethys Polar S",[("ammoni",20),("water",15)]),
    (1,1,"dust","Tethys Deep",[("ammoni",80),("water",70),("methn",20)]),
    (2,1,"dust","Tethys Vent",[("ammoni",60),("water",50),("methn",15)]),
    (3,1,"barren","Tethys Shelf",[("ammoni",25),("water",20)]),
    (4,1,"barren","Tethys Far",[("ammoni",15),("water",10)]),
]

MOON_GRIDS = {
    "selene":   (SELENE_DATA,   5, 3),
    "boreas":   (BOREAS_DATA,   5, 2),
    "zephyr":   (ZEPHYR_DATA,   5, 2),
    "notus":    (NOTUS_DATA,    5, 3),
    "eurus":    (EURUS_DATA,    4, 3),
    "crucible": (CRUCIBLE_DATA, 5, 3),
    "quench":   (QUENCH_DATA,   5, 2),
    "nereid":   (NEREID_DATA,   5, 2),
    "tethys":   (TETHYS_DATA,   5, 2),
}

FAC_NAMES = {
    1: "United Star Nations",
    2: "Northwind", 3: "Greenwell", 4: "Rivermark", 5: "Sundock", 6: "Copse",
    7: "Ironclad", 8: "Oreline", 9: "Basalt", 10: "Silicate", 11: "Fission",
}

# Build APS list using randomized IDs
APS = [
    (AP_ID["HelCin"], "Helios-Cinder Alderson point", SYS["Helios"], "42", SYS["Cinder"], AP_ID["CinHel"], AP_ORB["HelCin"]),
    (AP_ID["CinHel"], "Cinder-Helios Alderson point", SYS["Cinder"], "45", SYS["Helios"], AP_ID["HelCin"], AP_ORB["CinHel"]),
    (AP_ID["CinEmb"], "Cinder-Ember Alderson point", SYS["Cinder"], "48", SYS["Ember"], AP_ID["EmbCin"], AP_ORB["CinEmb"]),
    (AP_ID["EmbCin"], "Ember-Cinder Alderson point", SYS["Ember"], "40", SYS["Cinder"], AP_ID["CinEmb"], AP_ORB["EmbCin"]),
    (AP_ID["CinGle"], "Cinder-Gleam Alderson point", SYS["Cinder"], "52", SYS["Gleam"], AP_ID["GleCin"], AP_ORB["CinGle"]),
    (AP_ID["GleCin"], "Gleam-Cinder Alderson point", SYS["Gleam"], "38", SYS["Cinder"], AP_ID["CinGle"], AP_ORB["GleCin"]),
    (AP_ID["CinAsh"], "Cinder-Ash Alderson point", SYS["Cinder"], "58", SYS["Ash"], AP_ID["AshCin"], AP_ORB["CinAsh"]),
    (AP_ID["AshCin"], "Ash-Cinder Alderson point", SYS["Ash"], "70", SYS["Cinder"], AP_ID["CinAsh"], AP_ORB["AshCin"]),
    (AP_ID["FomSha"], "Fomal-Shards Alderson point", SYS["Fomal"], "44", SYS["Shards"], AP_ID["ShaFom"], AP_ORB["FomSha"]),
    (AP_ID["ShaFom"], "Shards-Fomal Alderson point", SYS["Shards"], "46", SYS["Fomal"], AP_ID["FomSha"], AP_ORB["ShaFom"]),
    (AP_ID["ShaDee"], "Shards-Deep Alderson point", SYS["Shards"], "50", SYS["Deep"], AP_ID["DeeSha"], AP_ORB["ShaDee"]),
    (AP_ID["DeeSha"], "Deep-Shards Alderson point", SYS["Deep"], "55", SYS["Shards"], AP_ID["ShaDee"], AP_ORB["DeeSha"]),
    (AP_ID["ShaGra"], "Shards-Graph Alderson point", SYS["Shards"], "54", SYS["Graph"], AP_ID["GraSha"], AP_ORB["ShaGra"]),
    (AP_ID["GraSha"], "Graph-Shards Alderson point", SYS["Graph"], "42", SYS["Shards"], AP_ID["ShaGra"], AP_ORB["GraSha"]),
    (AP_ID["ShaSpa"], "Shards-Spare Alderson point", SYS["Shards"], "60", SYS["Spare"], AP_ID["SpaSha"], AP_ORB["ShaSpa"]),
    (AP_ID["SpaSha"], "Spare-Shards Alderson point", SYS["Spare"], "40", SYS["Shards"], AP_ID["ShaSpa"], AP_ORB["SpaSha"]),
    (AP_ID["CinShaU"], "Cinder-Shards UAP", SYS["Cinder"], "55", SYS["Shards"], AP_ID["ShaCinU"], AP_ORB["CinShaU"]),
    (AP_ID["ShaCinU"], "Shards-Cinder UAP", SYS["Shards"], "58", SYS["Cinder"], AP_ID["CinShaU"], AP_ORB["ShaCinU"]),
]

# Shorthand region lookups
def R_arbor(x, y): return REG[("arbor", x, y)]
def R_anvil(x, y): return REG[("anvil", x, y)]
def R_scoria(x, y): return REG[("scoria", x, y)]
def R_pyre(x, y): return REG[("pyre", x, y)]
def R_moon(mname, x, y): return REG[(mname, x, y)]
def R_empty(key): return REG[("empty", key)]

# Semantic space exit keys — all SPACE_PAIRS use semantic region lookups
SPACE_PAIRS = [
    (R_arbor(5, 0), R_scoria(0, 0), 8),
    (R_scoria(0, 0), R_arbor(5, 0), 8),
    (R_arbor(5, 0), R_anvil(2, 0), 26),
    (R_anvil(2, 0), R_arbor(5, 0), 26),
    (R_anvil(2, 0), R_pyre(0, 0), 8),
    (R_pyre(0, 0), R_anvil(2, 0), 8),
    # Moon arrivals: first region of each moon <-> parent hub
    (R_moon("selene", 0, 0), R_arbor(5, 0), 4), (R_arbor(5, 0), R_moon("selene", 0, 0), 4),
    (R_moon("boreas", 0, 0), R_arbor(5, 0), 8), (R_arbor(5, 0), R_moon("boreas", 0, 0), 8),
    (R_moon("zephyr", 0, 0), R_arbor(5, 0), 8), (R_arbor(5, 0), R_moon("zephyr", 0, 0), 8),
    (R_moon("notus", 0, 0), R_arbor(5, 0), 8), (R_arbor(5, 0), R_moon("notus", 0, 0), 8),
    (R_moon("eurus", 0, 0), R_arbor(5, 0), 8), (R_arbor(5, 0), R_moon("eurus", 0, 0), 8),
    (R_moon("crucible", 0, 0), R_anvil(2, 0), 4), (R_anvil(2, 0), R_moon("crucible", 0, 0), 4),
    (R_moon("quench", 0, 0), R_anvil(2, 0), 4), (R_anvil(2, 0), R_moon("quench", 0, 0), 4),
    (R_moon("nereid", 0, 0), R_anvil(2, 0), 8), (R_anvil(2, 0), R_moon("nereid", 0, 0), 8),
    (R_moon("tethys", 0, 0), R_anvil(2, 0), 8), (R_anvil(2, 0), R_moon("tethys", 0, 0), 8),
]

UN_SPECS = {
    "Assembly": {
        "food_up": 600, "cash": 8000, "terran": 80,
        "buy_food": (500, 1), "buy_mod": ("farms", 2, 100),
        "sell_food": (120, 4), "sell_terran": (50, 50),
        "sell_titani": (10, 25), "buy_titani": (50, 8), "buy_copper": (30, 10),
        "farms": 24, "cplant": 10, "wnplnt": 5, "inftry": 4, "granary_food": 4000,
        "cplant_fuel": True, "anvil": False,
    },
    "Tidewatch": {
        "food_up": 200, "cash": 2000, "terran": 20,
        "buy_food": (80, 1), "buy_mod": None, "sell_food": (20, 5), "sell_terran": (10, 50),
        "farms": 6, "cplant": 2, "wnplnt": 0, "inftry": 1, "granary_food": 800,
        "cplant_fuel": True, "anvil": False,
    },
    "Windgap": {
        "food_up": 100, "cash": 1000, "terran": 12,
        "buy_food": (40, 1), "buy_mod": None, "sell_food": (10, 5), "sell_terran": (8, 50),
        "farms": 4, "cplant": 0, "wnplnt": 6, "inftry": 1, "granary_food": 400,
        "cplant_fuel": False, "anvil": False,
    },
    "Slagport": {
        "food_up": 200, "cash": 1500, "terran": 18,
        "buy_food": (200, 2), "buy_mod": None, "sell_food": (50, 6), "sell_terran": (12, 50),
        "sell_titani": (30, 5), "sell_copper": (20, 6),
        "buy_carbon": (40, 8), "buy_oil": (20, 12),
        "farms": 6, "cplant": 0, "wnplnt": 10, "inftry": 1, "granary_food": 300,
        "cplant_fuel": False, "anvil": True,
    },
    "Ridge": {
        "food_up": 100, "cash": 800, "terran": 10,
        "buy_food": (40, 2), "buy_mod": None, "sell_food": None, "sell_terran": (6, 50),
        "farms": 4, "cplant": 0, "wnplnt": 6, "inftry": 1, "granary_food": 200,
        "cplant_fuel": False, "anvil": True,
    },
    "Isotope": {
        "food_up": 100, "cash": 800, "terran": 10,
        "buy_food": (40, 2), "buy_mod": None, "sell_food": None, "sell_terran": (6, 50),
        "farms": 4, "cplant": 0, "wnplnt": 4, "inftry": 1, "granary_food": 200,
        "cplant_fuel": False, "anvil": True,
    },
}


def grid_index(rows, width):
    m = {}
    for i, row in enumerate(rows):
        x, y = row[0], row[1]
        m[(x, y)] = i
    return m


def neighbor_exits(rows, width, idfn, wrap_x=False):
    idx = {(r[0], r[1]): r for r in rows}
    out = {}
    for x, y, typ, *_ in rows:
        rid = idfn(x, y)
        exits = []
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if wrap_x and dy == 0:
                nx = nx % width
            n = idx.get((nx, ny))
            if not n:
                continue
            nid = idfn(n[0], n[1])
            dur = max(DUR[typ], DUR[n[2]])
            exits.append((nid, "ground", dur))
        out[rid] = exits
    return out


def extra_space(rid, pairs):
    add = []
    for a, b, dur in pairs:
        if a == rid:
            add.append((b, "space", dur))
    return add


def caps_for(typ, occ):
    if typ == "grassl":
        if occ and occ[0] == "un" and occ[1] == "Assembly":
            return [("settlement", 16)]
        if occ and occ[0] == "hq":
            return [("settlement", 10)]
        if occ and occ[0] == "un":
            return [("settlement", 8)]
        return [("settlement", 8)]
    return []


def extraction_caps(typ):
    if typ in ("dust", "barren", "mountn"):
        return [("extraction", 3)]
    return []


def un_city_xml(name, city_qty, indent):
    spec = UN_SPECS[name]
    t = indent
    t1 = indent + "\t"
    t2 = indent + "\t\t"
    sid = STK[(name, "city")]
    lines = [f'{t}<modulestack name="{sid}" name-en="{esc(name)}" type="city" quantity="{city_qty}" faction="1">']
    lines.append(item("cash", spec["cash"], t1))
    lines.append(item("terran", spec["terran"], t1))
    lines.append(nest_upkeep(spec["cash"] // 4 + 200, t1, spec["food_up"]))
    bf, bp = spec["buy_food"]
    lines.append(f'{t1}<buying item="food" quantity="{bf}" price="{bp}" />')
    if spec["buy_mod"]:
        m, q, p = spec["buy_mod"]
        lines.append(f'{t1}<buying module="{m}" quantity="{q}" price="{p}" />')
    if spec["sell_food"]:
        q, p = spec["sell_food"]
        lines.append(f'{t1}<selling item="food" quantity="{q}" price="{p}" />')
    q, p = spec["sell_terran"]
    lines.append(f'{t1}<selling item="terran" quantity="{q}" price="{p}" />')
    for res in ("titani", "copper"):
        key = f"sell_{res}"
        if key in spec and spec[key]:
            q, p = spec[key]
            lines.append(f'{t1}<selling item="{res}" quantity="{q}" price="{p}" />')
    for res in ("titani", "copper", "carbon", "oil"):
        key = f"buy_{res}"
        if key in spec and spec[key]:
            q, p = spec[key]
            lines.append(f'{t1}<buying item="{res}" quantity="{q}" price="{p}" />')
    lines.append(f'{t1}<modulestack name="{STK[(name, "garrison")]}" name-en="{esc(name)} garrison" type="inftry" quantity="{spec["inftry"]}" faction="1">')
    lines.append(item("rctlnc", spec["inftry"], t2))
    lines.append(nest_upkeep(5 * spec["inftry"], t2))
    lines.append(f"{t1}</modulestack>")
    lines.append(f'{t1}<modulestack name="{STK[(name, "farms")]}" name-en="{esc(name)} farms" type="farms" quantity="{spec["farms"]}" faction="1">')
    lines.append(item("terran", spec["farms"] * 5, t2))
    lines.append(nest_upkeep(spec["farms"] * 55, t2))
    lines.append(f"{t1}</modulestack>")
    if spec["cplant"]:
        lines.append(f'{t1}<modulestack name="{STK[(name, "cplant")]}" name-en="{esc(name)} coal plants" type="cplant" quantity="{spec["cplant"]}" faction="1">')
        lines.append(item("carbon", spec["cplant"] * 20, t2))
        lines.append(item("terran", spec["cplant"] * 3, t2))
        lines.append(nest_upkeep(spec["cplant"] * 53, t2))
        lines.append(f"{t1}</modulestack>")
    if spec["wnplnt"]:
        lines.append(f'{t1}<modulestack name="{STK[(name, "wnplnt")]}" name-en="{esc(name)} wind plants" type="wnplnt" quantity="{spec["wnplnt"]}" faction="1">')
        lines.append(nest_upkeep(spec["wnplnt"] * 10, t2))
        lines.append(f"{t1}</modulestack>")
    lines.append(f'{t1}<modulestack name="{STK[(name, "granary")]}" name-en="{esc(name)} granary" type="cargob" quantity="2" faction="1">')
    lines.append(item("food", spec["granary_food"], t2))
    if spec["anvil"]:
        lines.append(item("titani", 40, t2))
        lines.append(item("copper", 20, t2))
    lines.append(item("terran", 4, t2))
    lines.append(nest_upkeep(66, t2))
    lines.append(f"{t1}</modulestack>")
    lines.append(f"{t}</modulestack>")
    return "\n".join(lines)


def hq_xml(fac, indent):
    arbor = fac <= 6
    name = FAC_NAMES[fac]
    t, t1, t2 = indent, indent + "\t", indent + "\t\t"
    lines = [f'{t}<modulestack name="{STK[("hq", fac, "hq")]}" name-en="{esc(name)} HQ" type="corphq" quantity="1" faction="{fac}">']
    lines.append(f'{t1}<person name="{STK[("hq", fac, "person")]}" name-en="{esc(name)} CEO" race="terran" faction="{fac}">')
    lines.append(nest_upkeep(10, t2))
    lines.append(f"{t1}</person>")
    lines.append(item("terran", 40, t1))
    lines.append(nest_upkeep(210, t1))
    lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "cargo")]}" type="cargob" quantity="2" faction="{fac}">')
    if arbor:
        cargo = [("food", 400), ("terair", 200), ("h2o2", 200), ("iron", 40), ("carbon", 40), ("silici", 10)]
    else:
        cargo = [("food", 80), ("terair", 200), ("h2o2", 80), ("iron", 15), ("titani", 40), ("silici", 40), ("copper", 30), ("uraniu", 20)]
    for typ, q in cargo:
        lines.append(item(typ, q, t2))
    lines.append(item("terran", 6, t2))
    lines.append(nest_upkeep(80, t2))
    lines.append(f"{t1}</modulestack>")
    lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "farms")]}" type="farms" quantity="3" faction="{fac}">')
    lines.append(item("terran", 15, t2))
    lines.append(nest_upkeep(165, t2))
    lines.append(f"{t1}</modulestack>")
    lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "cdrill")]}" type="cdrill" quantity="1" faction="{fac}">')
    lines.append(item("terran", 6, t2))
    lines.append(nest_upkeep(46, t2))
    lines.append(f"{t1}</modulestack>")
    lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "factry")]}" type="factry" quantity="2" faction="{fac}">')
    lines.append(item("terran", 20, t2))
    lines.append(item("iron", 10, t2))
    lines.append(nest_upkeep(140, t2))
    lines.append(f"{t1}</modulestack>")
    if arbor:
        lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "energy")]}" type="cplant" quantity="2" faction="{fac}">')
        lines.append(item("carbon", 40, t2))
        lines.append(item("terran", 6, t2))
        lines.append(nest_upkeep(106, t2))
        lines.append(f"{t1}</modulestack>")
    else:
        lines.append(f'{t1}<modulestack name="{STK[("hq", fac, "energy")]}" type="wnplnt" quantity="8" faction="{fac}">')
        lines.append(nest_upkeep(80, t2))
        lines.append(f"{t1}</modulestack>")
    lines.append(f"{t}</modulestack>")
    return "\n".join(lines)


def region_block(rid, name, x, y, typ, res, caps, exits, stacks, indent="\t\t\t"):
    t = indent
    t1 = indent + "\t"
    lines = [f'{t}<region name="{rid}" name-en="{esc(name)}" X="{x}" Y="{y}" type="{typ}">']
    if caps:
        lines.append(capacities_xml(caps, t1))
    if exits:
        lines.append(exits_xml(exits, t1))
    if res:
        lines.append(resources_xml(res, t1))
    if stacks:
        lines.append(stacks)
    lines.append(f"{t}</region>")
    return "\n".join(lines)


def ap_planet(ap):
    pid, n, _sys, au, to_s, to_p, orb = ap
    unstable = ' unstable="true"' if "UAP" in n else ""
    body = [
        f'\t\t<planet name="{pid}" name-en="{esc(n)}" type="adpnt" AU="{au}" to-system="{to_s}" to-point="{to_p}"{unstable}>',
        f'\t\t\t<orbit name="{orb}" />',
        "\t\t</planet>",
    ]
    return "\n".join(body)


def simple_planet(pid, name, ptype, au, sx, sy, orbit, regions_xml, moons_xml=""):
    lines = [f'\t\t<planet name="{pid}" name-en="{esc(name)}" type="{ptype}" AU="{au}" surface-size-X="{sx}" surface-size-Y="{sy}">']
    if moons_xml:
        lines.append(moons_xml)
    lines.append(f'\t\t\t<orbit name="{orbit}" />')
    if regions_xml:
        lines.append(regions_xml)
    lines.append("\t\t</planet>")
    return "\n".join(lines)


def moon_xml(mid, name, mtype, au, orbit, sx=5, sy=3, regions_xml=""):
    lines = [f'\t\t\t<moon name="{mid}" name-en="{esc(name)}" type="{mtype}" AU="{au}" surface-size-X="{sx}" surface-size-Y="{sy}">']
    lines.append(f'\t\t\t\t<orbit name="{orbit}" />')
    if regions_xml:
        lines.append(regions_xml)
    lines.append("\t\t\t</moon>")
    return "\n".join(lines)


def emit_body_grid(rows, body_key, width, space_pairs, indent="\t\t\t\t"):
    """Emit region XML for a simple grid (no occupants). Uses wrap_x=True."""
    def idfn(x, y):
        return REG[(body_key, x, y)]

    full_rows = [(x, y, t, n, None, r) for x, y, t, n, r in rows]
    gex = neighbor_exits(full_rows, width, idfn, wrap_x=True)
    chunks = []
    for x, y, typ, name, res in rows:
        rid = idfn(x, y)
        exits = list(gex[rid]) + extra_space(rid, space_pairs)
        caps = extraction_caps(typ)
        chunks.append(region_block(rid, name, x, y, typ, res, caps, exits, None, indent))
    return "\n".join(chunks)


def space_for(rid):
    return extra_space(rid, SPACE_PAIRS)


def emit_grid(rows, body_key, width, indent="\t\t\t", wrap_x=False):
    def idfn(x, y):
        return REG[(body_key, x, y)]
    gex = neighbor_exits(rows, width, idfn, wrap_x=wrap_x)
    chunks = []
    for x, y, typ, name, occ, res in rows:
        rid = idfn(x, y)
        exits = list(gex[rid]) + space_for(rid)
        caps = caps_for(typ, occ)
        stacks = ""
        if occ and occ[0] == "un":
            stacks = un_city_xml(occ[1], occ[2], indent + "\t")
        elif occ and occ[0] == "hq":
            stacks = hq_xml(occ[1], indent + "\t")
        chunks.append(region_block(rid, name, x, y, typ, res, caps, exits, stacks, indent))
    return "\n".join(chunks)


def factions():
    lines = []
    for i in range(1, 12):
        pw = "" if i == 1 else f"pass{i}"
        em = "" if i == 1 else f"player{i}@example.invalid"
        bal = "0" if i == 1 else "10000"
        cl = "0" if i == 1 else "10000"
        lines.append(
            f'\t<faction name="{i}" name-en="{esc(FAC_NAMES[i])}" password="{pw}" email="{em}" '
            f'default-attitude="2" text-report="True" text-report-line-length="100" xml-report="True" '
            f'balance="{bal}" credit-line="{cl}" credit-rate="0.2" deposit-rate="0.05" />'
        )
    return "\n".join(lines)


def aps_for(system_id):
    return [a for a in APS if a[2] == system_id]


def main():
    arbor_regions = emit_grid(ARBOR, "arbor", 6, wrap_x=True)
    anvil_regions = emit_grid(ANVIL, "anvil", 7, wrap_x=True)

    scoria_regions = emit_body_grid(SCORIA, "scoria", 6, SPACE_PAIRS)
    pyre_regions = emit_body_grid(PYRE, "pyre", 5, SPACE_PAIRS)

    def moon_regions(mname):
        data, w, h = MOON_GRIDS[mname]
        return emit_body_grid(data, mname, w, SPACE_PAIRS, indent="\t\t\t\t")

    def mk_moon(mname, display_name, mtype, au, orbit_key):
        data, w, h = MOON_GRIDS[mname]
        reg = moon_regions(mname)
        return moon_xml(MOON[display_name], display_name, mtype, au, ORB[orbit_key], sx=w, sy=h, regions_xml=reg)

    def empty_landing(reg_key, name, typ, res):
        rid = R_empty(reg_key)
        return region_block(
            rid, name, 0, 0, typ, res, [("extraction", 4)],
            [(b, "space", dur) for a, b, dur in SPACE_PAIRS if a == rid],
            None,
        )

    helios_aps = "\n".join(ap_planet(a) for a in aps_for(SYS["Helios"]))
    fomal_aps = "\n".join(ap_planet(a) for a in aps_for(SYS["Fomal"]))

    parts = []
    parts.append('<?xml version="1.0" encoding="windows-1251"?>')
    parts.append('<game turn="1">')
    parts.append(factions())
    parts.append("\t<contracts />")
    parts.append("\t<galaxy>")

    # SS0001 Helios
    parts.append(f'\t\t<system name="{SYS["Helios"]}" name-en="Helios" X="0" Y="0" Z="0">')
    parts.append(f'\t\t\t<star name="{STAR["Helios"]}" name-en="Helios" type="M4" mass="1" />')
    selene = mk_moon("selene", "Selene", "rock", "0.0026", "Selene")
    parts.append(simple_planet(PLN["Arbor"], "Arbor", "ocean", "1", "6", "6", ORB["Arbor"], arbor_regions, selene))
    parts.append(simple_planet(PLN["Scoria"], "Scoria", "dust", "1.5", "6", "4", ORB["Scoria"], scoria_regions))
    parts.append(simple_planet(PLN["HeliosBelt"], "Helios belt", "abelt", "2.7", "8", "4", ORB["HeliosBelt"], ""))
    aeolus_moons = "\n".join([
        mk_moon("boreas", "Boreas", "ice", "0.01", "Boreas"),
        mk_moon("zephyr", "Zephyr", "ice", "0.015", "Zephyr"),
        mk_moon("notus", "Notus", "rock", "0.025", "Notus"),
        mk_moon("eurus", "Eurus", "vulcan", "0.04", "Eurus"),
    ])
    parts.append(simple_planet(PLN["Aeolus"], "Aeolus", "gasgnt", "5.2", "0", "0", ORB["Aeolus"], "", aeolus_moons))
    parts.append(helios_aps)
    parts.append("\t\t</system>")

    # SS0002 Fomal
    parts.append(f'\t\t<system name="{SYS["Fomal"]}" name-en="Fomal" X="1" Y="0" Z="0">')
    parts.append(f'\t\t\t<star name="{STAR["Fomal"]}" name-en="Fomal" type="M4" mass="1.2" />')
    anvil_moons = "\n".join([
        mk_moon("crucible", "Crucible", "rock", "0.002", "Crucible"),
        mk_moon("quench", "Quench", "ice", "0.003", "Quench"),
    ])
    parts.append(simple_planet(PLN["Anvil"], "Anvil", "ocean", "1.4", "7", "5", ORB["Anvil"], anvil_regions, anvil_moons))
    parts.append(simple_planet(PLN["Pyre"], "Pyre", "dust", "0.6", "5", "4", ORB["Pyre"], pyre_regions))
    parts.append(simple_planet(PLN["FomalBelt"], "Fomal belt", "abelt", "2.5", "8", "4", ORB["FomalBelt"], ""))
    fomal_giant_moons = "\n".join([
        mk_moon("nereid", "Nereid", "ice", "0.02", "Nereid"),
        mk_moon("tethys", "Tethys", "ice", "0.035", "Tethys"),
    ])
    parts.append(simple_planet(PLN["FomalGiant"], "Fomal giant", "gasgnt", "6", "0", "0", ORB["FomalGiant"], "", fomal_giant_moons))
    parts.append(fomal_aps)
    parts.append("\t\t</system>")

    empty = [
        (SYS["Ember"], "Ember", "-4", "1", "0", STAR["Ember"], [
            (PLN["EmberDust"], "Ember dust", "dust", "0.7", "4", "3", ORB["EmberDust"], empty_landing("EmberBrine", "Ember brine", "dust", [("lithia", 60), ("titani", 30), ("silici", 25)])),
            (PLN["EmberBelt"], "Ember belt", "abelt", "2.2", "4", "2", ORB["EmberBelt"], empty_landing("EmberRock", "Ember rock", "lrmast", [("iron", 40), ("titani", 20)])),
            (PLN["EmberGiant"], "Ember giant", "gasgnt", "4.1", "0", "0", ORB["EmberGiant"], ""),
        ], SYS["Ember"]),
        (SYS["Gleam"], "Gleam", "-4", "0", "0", STAR["Gleam"], [
            (PLN["GleamInner"], "Gleam inner", "dust", "0.12", "3", "2", ORB["GleamInner"], empty_landing("GleamCrust", "Gleam crust", "barren", [("silici", 30), ("copper", 20)])),
            (PLN["GleamBelt"], "Gleam belt", "abelt", "0.4", "4", "2", ORB["GleamBelt"], empty_landing("GleamMetal", "Gleam metal", "lrmast", [("reeox", 50), ("nickfe", 40), ("uraniu", 40), ("copper", 25)])),
            (PLN["GleamIce"], "Gleam ice", "dust", "0.8", "3", "2", ORB["GleamIce"], empty_landing("GleamIceLand", "Gleam ice", "dust", [("h2o2", 30)])),
        ], SYS["Gleam"]),
        (SYS["Cinder"], "Cinder", "-2", "0", "0", STAR["Cinder"], [
            (PLN["CinderDust"], "Cinder dust", "dust", "1.2", "5", "3", ORB["CinderDust"], empty_landing("CinderFumarole", "Cinder fumarole", "dust", [("boron", 50), ("carbon", 30), ("uraniu", 20), ("tungst", 8)])),
            (PLN["CinderBelt"], "Cinder belt", "abelt", "2.8", "4", "2", ORB["CinderBelt"], empty_landing("CinderCarbon", "Cinder carbon", "lrcast", [("grphit", 50), ("carbon", 40)])),
            (PLN["CinderGiant"], "Cinder giant", "gasgnt", "8", "0", "0", ORB["CinderGiant"], ""),
        ], SYS["Cinder"]),
        (SYS["Ash"], "Ash", "-4", "-1", "0", STAR["Ash"], [
            (PLN["AshIce"], "Ash ice", "dust", "20", "3", "2", ORB["AshIce"], empty_landing("AshXenon", "Ash xenon ice", "dust", [("xenon", 40), ("heliu3", 30), ("ammoni", 20), ("volatl", 20)])),
            (PLN["AshInnerGiant"], "Ash inner giant", "gasgnt", "6", "0", "0", ORB["AshInnerGiant"], ""),
            (PLN["AshOuterGiant"], "Ash outer giant", "gasgnt", "18", "0", "0", ORB["AshOuterGiant"], ""),
        ], SYS["Ash"]),
        (SYS["Shards"], "Shards", "3", "0", "0", STAR["Shards"], [
            (PLN["ShardsDust"], "Shards dust", "dust", "0.9", "4", "3", ORB["ShardsDust"], empty_landing("ShardsDustLand", "Shards evaporite", "dust", [("nitrat", 60), ("silici", 25)])),
            (PLN["ShardsInnerBelt"], "Shards inner belt", "abelt", "2.0", "4", "2", ORB["ShardsInnerBelt"], empty_landing("ShardsOrganics", "Shards organics", "lrcast", [("carbon", 40), ("kerogn", 25), ("oil", 15)])),
            (PLN["ShardsOuterBelt"], "Shards outer belt", "abelt", "3.1", "4", "2", ORB["ShardsOuterBelt"], empty_landing("ShardsIce", "Shards ice", "smcast", [("h2o2", 30), ("volatl", 20)])),
        ], SYS["Shards"]),
        (SYS["Deep"], "Deep", "5", "1", "0", STAR["Deep"], [
            (PLN["DeepDust"], "Deep dust", "dust", "0.5", "4", "3", ORB["DeepDust"], empty_landing("DeepMethane", "Deep methane ice", "dust", [("methn", 50), ("h2o2", 30), ("ammoni", 15)])),
            (PLN["DeepInnerGiant"], "Deep inner giant", "gasgnt", "4.5", "0", "0", ORB["DeepInnerGiant"], ""),
            (PLN["DeepOuterGiant"], "Deep outer giant", "gasgnt", "9.2", "0", "0", ORB["DeepOuterGiant"], ""),
        ], SYS["Deep"]),
        (SYS["Graph"], "Graph", "5", "0", "0", STAR["Graph"], [
            (PLN["Graph"], "Graph", "ocean", "0.95", "4", "3", ORB["Graph"], region_block(
                R_empty("GraphPrairie"), "Graph prairie", 0, 0, "grassl",
                [("terair", 100), ("food", 400), ("water", 200), ("carbon", 30), ("silici", 20), ("iron", 20)],
                [("settlement", 8)],
                [(b, "space", dur) for a, b, dur in SPACE_PAIRS if a == R_empty("GraphPrairie")],
                None,
            )),
            (PLN["GraphDust"], "Graph dust", "dust", "1.6", "3", "2", ORB["GraphDust"], empty_landing("GraphHighland", "Graph highland", "dust", [("silici", 30), ("alumin", 20), ("gold", 5)])),
            (PLN["GraphBelt"], "Graph belt", "abelt", "2.5", "4", "2", ORB["GraphBelt"], empty_landing("GraphRock", "Graph rock", "lrmast", [("iron", 40), ("silici", 20)])),
        ], SYS["Graph"]),
        (SYS["Spare"], "Spare", "5", "-1", "0", STAR["Spare"], [
            (PLN["SpareDust"], "Spare dust", "dust", "1.1", "4", "3", ORB["SpareDust"], empty_landing("SpareBeryllium", "Spare beryllium crust", "dust", [("berylm", 50), ("titani", 30), ("copper", 20)])),
            (PLN["SpareBelt"], "Spare belt", "abelt", "2.4", "4", "2", ORB["SpareBelt"], empty_landing("SpareIce", "Spare ice", "smcast", [("h2o2", 25)])),
        ], SYS["Spare"]),
    ]

    for sid, sname, x, y, z, star, planets, apsys in empty:
        parts.append(f'\t\t<system name="{sid}" name-en="{esc(sname)}" X="{x}" Y="{y}" Z="{z}">')
        parts.append(f'\t\t\t<star name="{star}" name-en="{esc(sname)}" type="M4" mass="1" />')
        for pl in planets:
            parts.append(simple_planet(*pl))
        for a in aps_for(apsys):
            parts.append(ap_planet(a))
        parts.append("\t\t</system>")

    parts.append("\t</galaxy>")
    parts.append("\t<orders />")
    parts.append("</game>")
    text = "\n".join(parts) + "\n"
    OUT.write_text(text, encoding="utf-8")
    print("wrote", OUT, "bytes", OUT.stat().st_size, "lines", text.count(chr(10)))


if __name__ == "__main__":
    main()
