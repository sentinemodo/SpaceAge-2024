#!/usr/bin/env python3
"""Emit campaign/gamein.1.xml (Windows-1251) from designer/galaxy.md.

Seed rules (this slice): no regional terair or h2o2; east-west wrap on
rectangular grids; no region-to-orbit exits; no region-to-alderson exits;
moon AU rounded to 3 decimals and at least 0.001; exits that touch
sea/ocean are naval (no coastal type); native race is on planet/moon
(`<race>`), not on orbit; asteroid belts are `<belt>` with `<composition>`
(no orbit, no child regions, no spawned rocks); Alderson Gates are
`<alderson>` (orbit, no corona region, no region hops; JUMP between the pair).

Regenerate:
    python campaign/_gen_gamein.py
"""
from __future__ import annotations

import os
import sys
import xml.etree.ElementTree as ET
from collections import OrderedDict

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_PATH = os.path.join(HERE, "gamein.1.xml")

# Item types present in campaign/data.xml. hydzn is refined, never a region ore.
LIVE_ITEMS = {
    "iron",
    "titani",
    "food",
    "carbon",
    "silici",
    "uraniu",
    "terair",
    "oil",
    "water",
    "h2o2",
    "copper",
    "gold",
    "nickfe",
    "heliu3",
    "cash",
    "tungst",
    "deutrm",
    "ammoni",
    "methn",
    "volatl",
    "kerogn",
    "alumin",
    "platnm",
    "lithia",
    "boron",
    "berylm",
    "xenon",
    "reeox",
    "grphit",
    "nitrat",
}

# Atmosphere and fuel are produced, not mined from the map. HQ cargo still
# lists terair/h2o2 via LIVE_ITEMS; regional deposits skip them.
REGION_SKIP = frozenset(("terair", "h2o2"))


def live_res(pairs):
    merged = OrderedDict()
    for typ, qty in pairs:
        if qty > 0 and typ in LIVE_ITEMS and typ not in REGION_SKIP:
            merged[typ] = merged.get(typ, 0) + qty
    return list(merged.items())


def format_moon_au(au):
    rounded = round(float(au), 3)
    if rounded < 0.001:
        rounded = 0.001
    return "%.3f" % rounded


GROUND_DURATION = {
    "grassl": 3,
    "dust": 3,
    "barren": 3,
    "mountn": 5,
    "sea": 6,
    "ocean": 7,
    "smmast": 3,
    "smcast": 3,
    "lrmast": 4,
    "lrcast": 4,
    "orbit": 1,
}

LIQUID_TYPES = frozenset(("sea", "ocean"))

# Seeded placeholders; play/init-run.ps1 may replace these later.
PLAYERS = OrderedDict(
    [
        (2, ("Northwind", "northwnd")),
        (3, ("Greenwell", "grnwell")),
        (4, ("Rivermark", "rivrmrk")),
        (5, ("Sundock", "sundock")),
        (6, ("Copse", "copse1")),
        (7, ("Ironclad", "irnclad")),
        (8, ("Oreline", "oreline")),
        (9, ("Basalt", "basalt")),
        (10, ("Silicate", "silicat")),
        (11, ("Fission", "fission")),
    ]
)

ARBOR = [
    # id is implied as R00001 + Y*6+X; listed for the name/type/resources/occupant
    ((0, 0), "ocean", "West Pelagic", [("terair", 100), ("water", 600), ("food", 40)]),
    ((1, 0), "sea", "Shelf", [("terair", 100), ("water", 400), ("food", 80), ("h2o2", 100)]),
    ((2, 0), "grassl", "Tidewatch Coast", [("terair", 100), ("food", 500), ("oil", 30), ("iron", 15), ("water", 150)]),
    ((3, 0), "grassl", "South Vale", [("terair", 100), ("food", 450), ("carbon", 25), ("iron", 20), ("water", 120)]),
    ((4, 0), "dust", "Launch Steppe", [("iron", 30), ("silici", 25), ("carbon", 10)]),
    ((5, 0), "barren", "Cinder Flats", [("iron", 20), ("silici", 15)]),
    ((0, 1), "ocean", "West Deep", [("terair", 100), ("water", 600), ("food", 40)]),
    ((1, 1), "grassl", "Northwind Grant", [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    ((2, 1), "grassl", "Mid Vale", [("terair", 100), ("food", 500), ("carbon", 20), ("iron", 15), ("water", 140)]),
    ((3, 1), "grassl", "Greenwell Grant", [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    ((4, 1), "mountn", "South Ridge", [("iron", 70), ("silici", 20), ("carbon", 5)]),
    ((5, 1), "dust", "East Dune", [("iron", 30), ("silici", 25), ("carbon", 10)]),
    ((0, 2), "sea", "West Coast", [("terair", 100), ("water", 400), ("food", 80), ("h2o2", 80)]),
    ((1, 2), "grassl", "Farm Belt", [("terair", 100), ("food", 700), ("carbon", 35), ("iron", 20), ("water", 160)]),
    ((2, 2), "grassl", "Assembly Basin", [("terair", 100), ("food", 800), ("carbon", 40), ("iron", 25), ("water", 180)]),
    ((3, 2), "grassl", "Central Basin", [("terair", 100), ("food", 550), ("carbon", 25), ("iron", 20), ("water", 150)]),
    ((4, 2), "grassl", "Rivermark Grant", [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    ((5, 2), "mountn", "East Peak", [("iron", 80), ("silici", 25)]),
    ((0, 3), "ocean", "Mid Pelagic", [("terair", 100), ("water", 600), ("food", 30)]),
    ((1, 3), "ocean", "Inner Pelagic", [("terair", 100), ("water", 600), ("food", 30)]),
    ((2, 3), "grassl", "Prairie", [("terair", 100), ("food", 500), ("carbon", 20), ("iron", 15), ("water", 140)]),
    ((3, 3), "grassl", "Sundock Grant", [("terair", 100), ("food", 600), ("oil", 20), ("iron", 20), ("water", 150)]),
    ((4, 3), "grassl", "East Steppe", [("terair", 100), ("food", 480), ("carbon", 20), ("iron", 18), ("water", 130)]),
    ((5, 3), "mountn", "East Crag", [("iron", 75), ("silici", 20)]),
    ((0, 4), "ocean", "North Pelagic", [("terair", 100), ("water", 600), ("food", 30)]),
    ((1, 4), "sea", "North Sound", [("terair", 100), ("water", 400), ("food", 70), ("h2o2", 90)]),
    ((2, 4), "grassl", "Copse Grant", [("terair", 100), ("food", 600), ("carbon", 30), ("iron", 20), ("water", 150)]),
    ((3, 4), "grassl", "Windgap", [("terair", 100), ("food", 450), ("carbon", 20), ("iron", 15), ("water", 140)]),
    ((4, 4), "dust", "Loess", [("iron", 35), ("silici", 20), ("carbon", 15)]),
    ((5, 4), "barren", "East Flat", [("iron", 20), ("silici", 15)]),
    ((0, 5), "ocean", "Polar Ocean", [("terair", 100), ("water", 700), ("food", 20)]),
    ((1, 5), "ocean", "Polar Ocean E", [("terair", 100), ("water", 700), ("food", 20)]),
    ((2, 5), "sea", "Polar Sea", [("terair", 100), ("water", 500), ("food", 40), ("h2o2", 120)]),
    ((3, 5), "grassl", "Tundra", [("terair", 100), ("food", 250), ("carbon", 10), ("iron", 10), ("water", 200)]),
    ((4, 5), "mountn", "North Spine", [("iron", 60), ("silici", 20)]),
    ((5, 5), "dust", "Polar Dust", [("iron", 25), ("silici", 20)]),
]

ANVIL = [
    ((0, 0), "ocean", "West Sea", [("terair", 100), ("water", 350), ("food", 10)]),
    ((1, 0), "sea", "West Shelf", [("terair", 100), ("water", 200), ("food", 30)]),
    ((2, 0), "dust", "Pad", [("copper", 25), ("silici", 40), ("titani", 20), ("iron", 25)]),
    ((3, 0), "grassl", "Slagport", [("terair", 100), ("food", 120), ("water", 80), ("iron", 20), ("silici", 30), ("titani", 20)]),
    ((4, 0), "mountn", "South Ore", [("titani", 60), ("silici", 40), ("copper", 30), ("iron", 30)]),
    ((5, 0), "dust", "South Dune", [("copper", 20), ("silici", 40), ("titani", 15), ("iron", 25)]),
    ((6, 0), "barren", "South Scarp", [("silici", 50), ("titani", 20)]),
    ((0, 1), "sea", "Northwest Sea", [("terair", 100), ("water", 200), ("food", 25)]),
    ((1, 1), "grassl", "Ironclad Grant", [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("titani", 25), ("silici", 30)]),
    ((2, 1), "grassl", "Slope", [("terair", 100), ("food", 100), ("water", 70), ("silici", 25), ("titani", 15)]),
    ((3, 1), "mountn", "West Spine", [("titani", 70), ("copper", 35), ("silici", 40), ("iron", 30)]),
    ((4, 1), "mountn", "Mid Spine", [("titani", 50), ("uraniu", 80), ("silici", 30), ("copper", 20)]),
    ((5, 1), "grassl", "Oreline Grant", [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("titani", 25), ("silici", 30)]),
    ((6, 1), "dust", "East Talus", [("copper", 25), ("silici", 40), ("titani", 20)]),
    ((0, 2), "grassl", "Marsh", [("terair", 100), ("food", 150), ("water", 100), ("iron", 15), ("silici", 20)]),
    ((1, 2), "grassl", "Bench", [("terair", 100), ("food", 110), ("water", 70), ("silici", 25), ("titani", 15)]),
    ((2, 2), "mountn", "Crag", [("titani", 80), ("copper", 40), ("silici", 45), ("iron", 35)]),
    ((3, 2), "grassl", "Basalt Grant", [("terair", 100), ("food", 140), ("water", 80), ("iron", 20), ("copper", 20), ("silici", 30)]),
    ((4, 2), "dust", "Scree", [("copper", 30), ("silici", 40), ("titani", 25), ("iron", 20)]),
    ((5, 2), "mountn", "East Peak", [("titani", 55), ("uraniu", 120), ("silici", 35), ("copper", 25)]),
    ((6, 2), "grassl", "Ridge", [("terair", 100), ("food", 100), ("water", 70), ("silici", 30), ("titani", 20), ("iron", 15)]),
    ((0, 3), "ocean", "Gulf", [("terair", 100), ("water", 350), ("food", 10)]),
    ((1, 3), "grassl", "Silicate Grant", [("terair", 100), ("food", 140), ("water", 80), ("silici", 40), ("titani", 20), ("iron", 15)]),
    ((2, 3), "grassl", "Vale", [("terair", 100), ("food", 90), ("water", 70), ("silici", 25), ("iron", 15)]),
    ((3, 3), "mountn", "Uraninite", [("uraniu", 150), ("titani", 40), ("silici", 30), ("copper", 20)]),
    ((4, 3), "grassl", "Thin Soil", [("terair", 100), ("food", 80), ("water", 60), ("silici", 20), ("titani", 10)]),
    ((5, 3), "grassl", "Fission Grant", [("terair", 100), ("food", 140), ("water", 80), ("uraniu", 15), ("titani", 20), ("silici", 30)]),
    ((6, 3), "dust", "Fan", [("copper", 20), ("silici", 45), ("titani", 15)]),
    ((0, 4), "ocean", "North Sea", [("terair", 100), ("water", 350), ("food", 8)]),
    ((1, 4), "sea", "North Shelf", [("terair", 100), ("water", 200), ("food", 20)]),
    ((2, 4), "dust", "Ash", [("copper", 15), ("silici", 35), ("titani", 15), ("iron", 20)]),
    ((3, 4), "grassl", "Isotope", [("terair", 100), ("food", 90), ("water", 70), ("silici", 25), ("titani", 15), ("iron", 15)]),
    ((4, 4), "mountn", "Shield", [("titani", 65), ("silici", 40), ("copper", 30), ("iron", 30)]),
    ((5, 4), "barren", "Glass", [("silici", 50), ("titani", 15)]),
    ((6, 4), "dust", "North Reg", [("copper", 20), ("silici", 40), ("iron", 20)]),
]


class Ids:
    def __init__(self):
        self.region = 1
        self.orbit = 1
        self.planet = 1
        self.moon = 1
        self.star = 1
        self._skip_r = {1490, 1491}
        self._skip_o = {110, 111}

    def skip_regions_to(self, n):
        self.region = max(self.region, n)

    def skip_planets_to(self, n):
        self.planet = max(self.planet, n)

    def R(self, n=None):
        if n is not None:
            self.region = max(self.region, n + 1)
            return "R%05d" % n
        while self.region in self._skip_r:
            self.region += 1
        n = self.region
        self.region += 1
        return "R%05d" % n

    def O(self, n=None):
        if n is not None:
            self.orbit = max(self.orbit, n + 1)
            return "O%05d" % n
        while self.orbit in self._skip_o:
            self.orbit += 1
        n = self.orbit
        self.orbit += 1
        return "O%05d" % n

    def P(self, n=None):
        if n is not None:
            self.planet = max(self.planet, n + 1)
            return "P%05d" % n
        n = self.planet
        self.planet += 1
        return "P%05d" % n

    def M(self):
        n = self.moon
        self.moon += 1
        return "M%05d" % n

    def S(self):
        n = self.star
        self.star += 1
        return "S%05d" % n


class Exit:
    def __init__(self, kind, target, mode, duration):
        self.kind = kind  # "region" | "orbit"
        self.target = target
        self.mode = mode
        self.duration = duration


class Region:
    def __init__(self, name, name_en, x, y, typ):
        self.name = name
        self.name_en = name_en
        self.x = x
        self.y = y
        self.typ = typ
        self.capacities = []  # (group, qty)
        self.resources = []  # (type, qty)
        self.exits = []
        self.stacks = []


class Orbit:
    def __init__(self, name, races=None, resources=None):
        self.name = name
        self.races = list(races or [])
        self.resources = list(resources or [])
        self.stacks = []


class Moon:
    def __init__(self, name, name_en, typ, au, sx, sy, gravity="low", temperature="cold", atmosphere="none"):
        self.name = name
        self.name_en = name_en
        self.typ = typ
        self.au = format_moon_au(au)
        self.sx = sx
        self.sy = sy
        self.gravity = gravity
        self.temperature = temperature
        self.atmosphere = atmosphere
        self.description = None
        self.orbit = None
        self.races = []
        self.regions = []


class Planet:
    def __init__(
        self,
        name,
        name_en,
        typ,
        au,
        sx,
        sy,
        gravity=None,
        temperature=None,
        atmosphere=None,
        pair=None,
    ):
        self.name = name
        self.name_en = name_en
        self.typ = typ
        self.au = au
        self.sx = sx
        self.sy = sy
        self.gravity = gravity
        self.temperature = temperature
        self.atmosphere = atmosphere
        self.pair = pair
        self.description = None
        self.orbit = None
        self.races = []
        self.moons = []
        self.belts = []
        self.regions = []


class Belt:
    def __init__(self, name, name_en, au, composition=None):
        self.name = name
        self.name_en = name_en
        self.au = au
        self.composition = list(composition or [])  # (type, qty, probability)
        self.description = None
        self.exits = []
        self.stacks = []


class Alderson:
    def __init__(self, name, name_en, au, pair, temperature="cold", atmosphere="none"):
        self.name = name
        self.name_en = name_en
        self.au = au
        self.pair = pair
        self.temperature = temperature
        self.atmosphere = atmosphere
        self.description = None
        self.orbit = None
        self.exits = []
        self.stacks = []


class Star:
    def __init__(self, name, name_en, typ="M4"):
        self.name = name
        self.name_en = name_en
        self.typ = typ
        self.description = None


class System:
    def __init__(self, name, name_en, x, y, z):
        self.name = name
        self.name_en = name_en
        self.x = x
        self.y = y
        self.z = z
        self.star = None
        self.planets = []
        self.belts = []
        self.aldersons = []


class Stack:
    def __init__(self, name, typ, faction, quantity=1, name_en=None):
        self.name = name
        self.typ = typ
        self.faction = str(faction)
        self.quantity = quantity
        self.name_en = name_en
        self.items = []
        self.upkeep = []
        self.buying = []
        self.selling = []
        self.people = []
        self.children = []


def el(parent, tag, **attrs):
    child = ET.SubElement(parent, tag)
    for key, value in attrs.items():
        if value is None:
            continue
        child.set(key, str(value))
    return child


def set_attrs(node, **attrs):
    for key, value in attrs.items():
        if value is None:
            continue
        node.set(key, str(value))


def add_items(node, items, tag="itemstack"):
    for typ, qty in items:
        el(node, tag, type=typ, quantity=qty)


def add_offers(node, offers, tag):
    for offer in offers:
        attrs = {"quantity": offer["quantity"], "price": offer["price"]}
        if "item" in offer:
            attrs["item"] = offer["item"]
        if "module" in offer:
            attrs["module"] = offer["module"]
        el(node, tag, **attrs)


def emit_stack(parent, stack):
    attrs = {
        "name": stack.name,
        "type": stack.typ,
        "quantity": stack.quantity,
        "faction": stack.faction,
    }
    if stack.name_en:
        attrs["name-en"] = stack.name_en
    node = el(parent, "modulestack", **attrs)
    for person in stack.people:
        p = el(
            node,
            "person",
            name=person["name"],
            **{"name-en": person["name-en"]},
            race=person.get("race", "terran"),
            faction=person["faction"],
        )
        add_items(p, person.get("upkeep", []), "upkeep")
    add_items(node, stack.items)
    add_items(node, stack.upkeep, "upkeep")
    add_offers(node, stack.buying, "buying")
    add_offers(node, stack.selling, "selling")
    for child in stack.children:
        emit_stack(node, child)
    return node


def emit_region(parent, region):
    node = el(
        parent,
        "region",
        name=region.name,
        **{"name-en": region.name_en},
        X=region.x,
        Y=region.y,
        type=region.typ,
    )
    for group, qty in region.capacities:
        el(node, "capacity", group=group, quantity=qty)
    for ex in region.exits:
        e = el(node, "exit", **{ex.kind: ex.target})
        el(e, "exitmode", mode=ex.mode, duration=ex.duration)
    for typ, qty in region.resources:
        el(node, "resource", type=typ, quantity=qty)
    for stack in region.stacks:
        emit_stack(node, stack)
    return node


def emit_orbit(parent, orbit):
    node = el(parent, "orbit", name=orbit.name)
    for race in orbit.races:
        el(node, "race", type=race)
    for typ, qty in orbit.resources:
        el(node, "resource", type=typ, quantity=qty)
    for stack in orbit.stacks:
        emit_stack(node, stack)
    return node


def with_description(attrs, obj):
    if getattr(obj, "description", None):
        attrs["description"] = obj.description
    return attrs


def emit_belt(parent, belt):
    attrs = with_description({"name": belt.name, "name-en": belt.name_en, "AU": belt.au}, belt)
    node = el(parent, "belt", **attrs)
    if belt.composition:
        comp = el(node, "composition")
        for typ, qty, probability in belt.composition:
            el(comp, "resource", type=typ, quantity=qty, probability=probability)
    for ex in belt.exits:
        e = el(node, "exit", **{ex.kind: ex.target})
        el(e, "exitmode", mode=ex.mode, duration=ex.duration)
    for stack in belt.stacks:
        emit_stack(node, stack)
    return node


def emit_alderson(parent, alderson):
    attrs = {
        "name": alderson.name,
        "name-en": alderson.name_en,
        "AU": alderson.au,
        "pair": alderson.pair,
        "temperature": alderson.temperature,
        "atmosphere": alderson.atmosphere,
    }
    node = el(parent, "alderson", **attrs)
    if alderson.orbit:
        emit_orbit(node, alderson.orbit)
    for ex in alderson.exits:
        e = el(node, "exit", **{ex.kind: ex.target})
        el(e, "exitmode", mode=ex.mode, duration=ex.duration)
    for stack in alderson.stacks:
        emit_stack(node, stack)
    return node


def emit_moon(parent, moon):
    attrs = {
        "name": moon.name,
        "name-en": moon.name_en,
        "type": moon.typ,
        "AU": moon.au,
        "surface-size-X": moon.sx,
        "surface-size-Y": moon.sy,
    }
    if moon.gravity:
        attrs["gravity"] = moon.gravity
    if moon.temperature:
        attrs["temperature"] = moon.temperature
    if moon.atmosphere:
        attrs["atmosphere"] = moon.atmosphere
    with_description(attrs, moon)
    node = el(parent, "moon", **attrs)
    for race in moon.races:
        el(node, "race", type=race)
    if moon.orbit:
        emit_orbit(node, moon.orbit)
    for region in moon.regions:
        emit_region(node, region)
    return node


def emit_planet(parent, planet):
    attrs = {
        "name": planet.name,
        "name-en": planet.name_en,
        "type": planet.typ,
        "AU": planet.au,
        "surface-size-X": planet.sx,
        "surface-size-Y": planet.sy,
    }
    if planet.gravity:
        attrs["gravity"] = planet.gravity
    if planet.temperature:
        attrs["temperature"] = planet.temperature
    if planet.atmosphere:
        attrs["atmosphere"] = planet.atmosphere
    if planet.pair:
        attrs["pair"] = planet.pair
    with_description(attrs, planet)
    node = el(parent, "planet", **attrs)
    for race in planet.races:
        el(node, "race", type=race)
    for moon in planet.moons:
        emit_moon(node, moon)
    for ring in planet.belts:
        emit_belt(node, ring)
    if planet.orbit:
        emit_orbit(node, planet.orbit)
    for region in planet.regions:
        emit_region(node, region)
    return node


def link_grid(regions):
    by_xy = {(r.x, r.y): r for r in regions}
    if not regions:
        return
    width = max(r.x for r in regions) + 1
    for region in regions:
        duration = GROUND_DURATION.get(region.typ, 3)
        seen = set()
        neighbours = [
            (region.x - 1, region.y),
            (region.x + 1, region.y),
            (region.x, region.y - 1),
            (region.x, region.y + 1),
        ]
        if width > 1:
            if region.x == 0:
                neighbours.append((width - 1, region.y))
            if region.x == width - 1:
                neighbours.append((0, region.y))
        for nx, ny in neighbours:
            if (nx, ny) in seen:
                continue
            seen.add((nx, ny))
            other = by_xy.get((nx, ny))
            if other is None or other is region:
                continue
            mode = "naval" if (region.typ in LIQUID_TYPES or other.typ in LIQUID_TYPES) else "ground"
            region.exits.append(Exit("region", other.name, mode, duration))


def add_space_pair(a, b, duration):
    def kind_of(obj):
        if isinstance(obj, Belt):
            return "belt"
        if isinstance(obj, Alderson):
            return "alderson"
        return "region"

    a.exits.append(Exit(kind_of(b), b.name, "space", duration))
    b.exits.append(Exit(kind_of(a), a.name, "space", duration))


def fill_grid(ids, rows, width, first_n, settlement_fn=None):
    regions = []
    for (x, y), typ, name_en, resources in rows:
        n = first_n + y * width + x
        region = Region(ids.R(n), name_en, x, y, typ)
        if settlement_fn:
            cap = settlement_fn(region, name_en, typ)
            if cap:
                region.capacities.append(("settlement", cap))
        region.resources = live_res(resources)
        regions.append(region)
    link_grid(regions)
    return regions


def settlement_arbor(region, name_en, typ):
    if typ != "grassl":
        return None
    if name_en == "Assembly Basin":
        return 16
    if name_en in (
        "Northwind Grant",
        "Greenwell Grant",
        "Rivermark Grant",
        "Sundock Grant",
        "Copse Grant",
        "Farm Belt",
        "Tidewatch Coast",
        "Windgap",
    ):
        return 8
    return 4


def settlement_anvil(region, name_en, typ):
    if typ != "grassl":
        return None
    if name_en in (
        "Ironclad Grant",
        "Oreline Grant",
        "Basalt Grant",
        "Silicate Grant",
        "Fission Grant",
        "Slagport",
        "Ridge",
        "Isotope",
        "Vale",
    ):
        return 8
    return 4


def nest(parent, name, typ, faction, quantity=1, name_en=None, items=None, upkeep=None):
    child = Stack(name, typ, faction, quantity, name_en)
    child.items = list(items or [])
    child.upkeep = list(upkeep or [])
    parent.children.append(child)
    return child


def city(
    name,
    name_en,
    faction,
    qty,
    cash,
    terran,
    food_upkeep,
    nested,
    buying=None,
    selling=None,
    extra_items=None,
):
    stack = Stack(name, "city", faction, qty, name_en)
    stack.items = [("cash", cash), ("terran", terran)] + list(extra_items or [])
    stack.upkeep = [("food", food_upkeep)]
    stack.buying = list(buying or [])
    stack.selling = list(selling or [])
    stack.children = nested
    return stack


def hq_stack(fac, planet):
    name_en, _pw = PLAYERS[fac]
    base = 200000 + (fac - 2) * 10000
    hq = Stack("%d" % (base + 1), "corphq", fac, 1, "%s Headquarters" % name_en)
    hq.people = [
        {
            "name": "%d" % (base + 10),
            "name-en": "%s CEO" % name_en,
            "race": "terran",
            "faction": str(fac),
            "upkeep": [("cash", 10)],
        }
    ]
    # corphq crew=20; nested production stacks carry their own terran crews so turn-1
    # CanOperate succeeds without stripping HQ below its own requirement.
    hq.items = [("terran", 20)]
    hq.upkeep = [("cash", 90)]
    if planet == "arbor":
        cargo = [("food", 400), ("terair", 200), ("h2o2", 200), ("iron", 40), ("carbon", 40), ("silici", 10)]
        nest(hq, "%d" % (base + 3), "cargob", fac, 2, items=cargo, upkeep=[("cash", 20)])
        nest(hq, "%d" % (base + 4), "cdrill", fac, 1, items=[("terran", 6)], upkeep=[("cash", 50)])
        nest(hq, "%d" % (base + 5), "factry", fac, 2, items=[("terran", 20)], upkeep=[("cash", 110)])
        nest(hq, "%d" % (base + 6), "farms", fac, 3, items=[("terran", 15)], upkeep=[("cash", 90)])
        nest(
            hq,
            "%d" % (base + 7),
            "cplant",
            fac,
            2,
            items=[("carbon", 20), ("terran", 4)],
            upkeep=[("cash", 80)],
        )
    else:
        cargo = [
            ("food", 80),
            ("terair", 200),
            ("h2o2", 80),
            ("iron", 15),
            ("titani", 40),
            ("silici", 40),
            ("copper", 30),
            ("uraniu", 20),
        ]
        nest(hq, "%d" % (base + 3), "cargob", fac, 2, items=cargo, upkeep=[("cash", 20)])
        nest(hq, "%d" % (base + 4), "cdrill", fac, 1, items=[("terran", 6)], upkeep=[("cash", 50)])
        nest(hq, "%d" % (base + 5), "factry", fac, 2, items=[("terran", 20)], upkeep=[("cash", 110)])
        nest(hq, "%d" % (base + 6), "farms", fac, 2, items=[("terran", 10)], upkeep=[("cash", 60)])
        nest(hq, "%d" % (base + 7), "wnplnt", fac, 8, upkeep=[("cash", 8)])
    return hq


def find_region(regions, name_en):
    for region in regions:
        if region.name_en == name_en:
            return region
    raise KeyError(name_en)


def find_id(regions, rid):
    for region in regions:
        if region.name == rid:
            return region
    raise KeyError(rid)


def make_pocket_grid(ids, width, height, namer, typer, resourcer, capacities=None):
    regions = []
    for y in range(height):
        for x in range(width):
            typ = typer(x, y)
            region = Region(ids.R(), namer(x, y), x, y, typ)
            if capacities:
                for cap in capacities(x, y, typ):
                    region.capacities.append(cap)
            region.resources = live_res(resourcer(x, y, typ))
            regions.append(region)
    link_grid(regions)
    return regions


def make_rect(ids, width, height, typ, name_prefix, resources_fn, capacities=None):
    def namer(x, y):
        return "%s %d,%d" % (name_prefix, x, y)

    def typer(_x, _y):
        return typ

    return make_pocket_grid(ids, width, height, namer, typer, resources_fn, capacities)


def build_world():
    ids = Ids()
    systems = []
    landings = {}

    helios = System("SS0001", "Helios", 0, 0, 0)
    helios.star = Star(ids.S(), "Helios")

    arbor = Planet("P00001", "Arbor", "ocean", 1.0, 6, 6, "normal", "habitable", "terair")
    arbor.races = ["terran"]
    arbor.orbit = Orbit(ids.O())
    arbor.regions = fill_grid(ids, ARBOR, 6, 1, settlement_arbor)
    landings["cinder"] = find_region(arbor.regions, "Cinder Flats")

    selene = Moon(ids.M(), "Selene", "rock", 0.0026, 5, 3, "low", "cold", "none")
    selene.orbit = Orbit(ids.O())
    arbor.moons.append(selene)

    # R00037–R00071 reserved for Anvil; Scoria landing is R00072.
    ids.skip_regions_to(72)
    scoria = Planet("P00002", "Scoria", "dust", 1.5, 6, 4, "low", "hot", "thin")
    scoria.orbit = Orbit(ids.O())

    def scoria_type(x, y):
        return ("dust", "mountn", "barren")[(x + y) % 3]

    def scoria_res(x, y, typ):
        return [("titani", 25 + x * 3), ("copper", 15 + y * 2), ("silici", 20), ("iron", 18)]

    def scoria_cap(x, y, typ):
        return [("extraction", 4)]

    scoria.regions = make_pocket_grid(
        ids, 6, 4, lambda x, y: "Scoria %d,%d" % (x, y), scoria_type, scoria_res, scoria_cap
    )
    scoria.regions[0].name_en = "Scoria landing"
    landings["scoria"] = scoria.regions[0]
    helios.planets.append(arbor)
    helios.planets.append(scoria)

    belt_h = Belt(
        "P00003",
        "Helios Belt",
        2.7,
        [
            ("uraniu", 80, 0.5),
            ("nickfe", 20, 0.5),
            ("iron", 15, 0.6),
            ("carbon", 30, 0.4),
            ("gold", 5, 0.1),
            ("oil", 8, 0.2),
        ],
    )
    landings["helios-belt"] = belt_h
    helios.belts.append(belt_h)

    # Selene after belt so first moon region is R00128.
    ids.skip_regions_to(128)

    def selene_type(x, y):
        if y == 0:
            return "dust"
        if x == 2:
            return "mountn"
        return "barren"

    def selene_res(x, y, typ):
        res = [("titani", 20 + x * 5), ("silici", 15 + y * 4)]
        if y == 0 or (x == 0 and y == 2):
            res.append(("water", 40 + y * 20))
        return res

    selene.regions = make_pocket_grid(
        ids, 5, 3, lambda x, y: "Selene %d,%d" % (x, y), selene_type, selene_res
    )

    aeolus = Planet("P00004", "Aeolus", "gasgnt", 5.2, 0, 0, "high", "cold", "hostile")
    aeolus.orbit = Orbit(ids.O(), resources=gas_cloud_resources("hostile", "Aeolus"))
    aeolus_moons = [
        ("Rime", "ice", [("water", 80), ("heliu3", 40)]),
        ("Glaze", "ice", [("water", 70), ("heliu3", 25)]),
        ("Shard", "rock", [("titani", 30), ("silici", 25), ("iron", 20)]),
        ("Cindercone", "vulcan", [("iron", 8), ("carbon", 5), ("silici", 20)]),
    ]
    for idx, (mname, mtyp, mres) in enumerate(aeolus_moons):
        moon = Moon(ids.M(), mname, mtyp, 0.003 + idx * 0.002, 5, 2, "low", "hot" if mtyp == "vulcan" else "cold", "none")
        moon.orbit = Orbit(ids.O())
        w, h = 5, 2

        def res_fn(x, y, typ, base=mres):
            out = list(base)
            if mtyp == "ice" and x < 2:
                out = [(t, q + 4) if t == "water" else (t, q) for t, q in out]
            return out

        moon.regions = make_rect(ids, w, h, "dust" if mtyp != "vulcan" else "mountn", mname, res_fn)
        aeolus.moons.append(moon)
        if mname == "Rime":
            landings["rime"] = moon.regions[0]
    aeolus.belts.append(
        Belt(
            "P00091",
            "Aeolus Ring",
            0.002,
            [("water", 40, 0.5), ("silici", 20, 0.4), ("iron", 10, 0.3)],
        )
    )
    helios.planets.append(aeolus)

    gate_h = Alderson("P00009", "Helios Fomal Gate", 80, "P00010")
    gate_h.orbit = Orbit("O00110")
    ids.O(110)
    landings["helios-gate"] = gate_h
    helios.aldersons.append(gate_h)
    systems.append(helios)

    fomal = System("SS0002", "Fomal", 1, 0, 0)
    fomal.star = Star(ids.S(), "Fomal")

    anvil = Planet("P00005", "Anvil", "ocean", 1.4, 7, 5, "normal", "habitable", "terair")
    anvil.races = ["terran"]
    anvil.orbit = Orbit(ids.O())
    anvil.regions = fill_grid(ids, ANVIL, 7, 37, settlement_anvil)
    landings["pad"] = find_region(anvil.regions, "Pad")

    anvil_rock = Moon(ids.M(), "Anvil Rock", "rock", 0.0024, 5, 3, "low", "cold", "none")
    anvil_rock.orbit = Orbit(ids.O())

    def arock_res(x, y, typ):
        res = [("titani", 18), ("silici", 22), ("iron", 12)]
        if y == 0 or x == 0:
            res.append(("water", 30 + x * 8))
        return res

    anvil_rock.regions = make_rect(ids, 5, 3, "dust", "Anvil Rock", arock_res)
    anvil.moons.append(anvil_rock)

    anvil_ice = Moon(ids.M(), "Anvil Ice", "ice", 0.004, 5, 2, "low", "cold", "none")
    anvil_ice.orbit = Orbit(ids.O())
    anvil_ice.regions = make_rect(
        ids, 5, 2, "dust", "Anvil Ice", lambda x, y, typ: [("water", 70 + x * 4), ("h2o2", 20), ("heliu3", 8)]
    )
    anvil.moons.append(anvil_ice)
    fomal.planets.append(anvil)

    pyre = Planet("P00006", "Pyre", "dust", 0.6, 5, 4, "low", "hot", "thin")
    pyre.orbit = Orbit(ids.O())

    def pyre_res(x, y, typ):
        return [("iron", 30 + x * 2), ("silici", 25 + y), ("titani", 8)]

    pyre.regions = make_pocket_grid(
        ids,
        5,
        4,
        lambda x, y: "Pyre %d,%d" % (x, y),
        lambda x, y: ("dust", "barren", "mountn")[(x + y) % 3],
        pyre_res,
        lambda x, y, typ: [("extraction", 4)],
    )
    pyre.regions[0].name_en = "Pyre landing"
    landings["pyre"] = pyre.regions[0]
    fomal.planets.append(pyre)

    belt_f = Belt(
        "P00007",
        "Fomal Belt",
        2.5,
        [("carbon", 40, 0.6), ("oil", 15, 0.4)],
    )
    landings["fomal-belt"] = belt_f
    fomal.belts.append(belt_f)

    giant_f = Planet("P00008", "Fomal Giant", "gasgnt", 6.0, 0, 0, "high", "cold", "hostile")
    giant_f.orbit = Orbit(ids.O(), resources=gas_cloud_resources("hostile", "Fomal Giant"))
    for mname, extra in (("Drift", [("heliu3", 20)]), ("Rimeband", [("heliu3", 30)])):
        moon = Moon(ids.M(), mname, "ice", 0.005, 5, 2, "low", "cold", "none")
        moon.orbit = Orbit(ids.O())
        moon.regions = make_rect(
            ids,
            5,
            2,
            "dust",
            mname,
            lambda x, y, typ, extra=extra: [("water", 60 + x * 5)] + extra,
        )
        giant_f.moons.append(moon)
    giant_f.belts.append(
        Belt(
            "P00092",
            "Fomal Ring",
            0.002,
            [("water", 30, 0.5), ("silici", 15, 0.3)],
        )
    )
    fomal.planets.append(giant_f)

    gate_f = Alderson("P00010", "Fomal Helios Gate", 80, "P00009")
    gate_f.orbit = Orbit("O00111")
    ids.O(111)
    landings["fomal-gate"] = gate_f
    fomal.aldersons.append(gate_f)
    systems.append(fomal)

    # Chemical hops between surface landings and belts (AU×drive not wired yet).
    # Same-body region↔orbit is implicit for space movers; do not emit orbit exits.
    # Gates are JUMP only — no region↔alderson exits.
    add_space_pair(landings["cinder"], landings["scoria"], 8)
    add_space_pair(landings["cinder"], landings["helios-belt"], 13)
    add_space_pair(landings["cinder"], landings["pad"], 26)
    add_space_pair(landings["pad"], landings["pyre"], 8)
    add_space_pair(landings["pad"], landings["fomal-belt"], 13)

    # Occupants
    assembly = city(
        "100001",
        "Assembly",
        1,
        6,
        8000,
        80,
        600,
        [
            Stack("100002", "farms", 1, 24, "Assembly farms"),
            Stack("100003", "cplant", 1, 10, "Assembly coal plants"),
            Stack("100004", "wnplnt", 1, 5, "Assembly wind plants"),
            Stack("100005", "inftry", 1, 4, "Assembly garrison"),
            Stack("100006", "cargob", 1, 2, "Assembly granary"),
        ],
        buying=[
            {"item": "food", "quantity": 500, "price": 1},
            {"item": "carbon", "quantity": 80, "price": 2},
            {"module": "farms", "quantity": 2, "price": 100},
        ],
        selling=[
            {"item": "food", "quantity": 120, "price": 4},
            {"item": "iron", "quantity": 30, "price": 3},
            {"item": "silici", "quantity": 15, "price": 4},
            {"item": "titani", "quantity": 10, "price": 6},
            {"item": "copper", "quantity": 8, "price": 6},
            {"item": "terair", "quantity": 50, "price": 1},
            {"item": "terran", "quantity": 50, "price": 50},
        ],
    )
    assembly.children[0].upkeep = [("cash", 720)]
    assembly.children[1].items = [("carbon", 80)]
    assembly.children[1].upkeep = [("cash", 400)]
    assembly.children[2].upkeep = [("cash", 5)]
    assembly.children[3].upkeep = [("cash", 60)]
    assembly.children[4].items = [
        ("food", 4000),
        ("iron", 30),
        ("silici", 15),
        ("titani", 10),
        ("copper", 8),
        ("terair", 50),
    ]
    assembly.children[4].upkeep = [("cash", 20)]
    find_region(arbor.regions, "Assembly Basin").stacks.append(assembly)

    def un_town(sid, name_en, qty, cash, terran, food_upkeep, farms, energy_typ, energy_qty, inftry, granary_items, buying, selling, extra_nested=None):
        children = [
            Stack("%d" % (sid + 1), "farms", 1, farms, "%s farms" % name_en),
            Stack("%d" % (sid + 2), energy_typ, 1, energy_qty, "%s power" % name_en),
            Stack("%d" % (sid + 3), "inftry", 1, inftry, "%s garrison" % name_en),
            Stack("%d" % (sid + 4), "cargob", 1, 1, "%s granary" % name_en),
        ]
        children[0].upkeep = [("cash", 30 * farms)]
        children[1].upkeep = [("cash", (40 if energy_typ == "cplant" else 1) * energy_qty)]
        if energy_typ == "cplant":
            children[1].items = [("carbon", 20)]
        children[2].upkeep = [("cash", 15 * inftry)]
        children[3].items = list(granary_items)
        children[3].upkeep = [("cash", 10)]
        if extra_nested:
            children.extend(extra_nested)
        return city("%d" % sid, name_en, 1, qty, cash, terran, food_upkeep, children, buying, selling)

    find_region(arbor.regions, "Tidewatch Coast").stacks.append(
        un_town(
            100010,
            "Tidewatch",
            2,
            1500,
            20,
            200,
            6,
            "cplant",
            2,
            1,
            [("food", 400), ("oil", 20)],
            [{"item": "food", "quantity": 80, "price": 1}],
            [{"item": "oil", "quantity": 15, "price": 5}],
        )
    )
    find_region(arbor.regions, "Windgap").stacks.append(
        un_town(
            100020,
            "Windgap",
            1,
            800,
            12,
            100,
            4,
            "wnplnt",
            6,
            1,
            [("food", 200)],
            [{"item": "food", "quantity": 100, "price": 1}],
            [{"item": "food", "quantity": 30, "price": 4}],
        )
    )
    find_region(anvil.regions, "Slagport").stacks.append(
        un_town(
            100030,
            "Slagport",
            2,
            1200,
            18,
            200,
            6,
            "wnplnt",
            10,
            1,
            [("food", 80), ("titani", 25), ("copper", 25), ("uraniu", 10), ("silici", 20)],
            [
                {"item": "food", "quantity": 200, "price": 2},
                {"item": "iron", "quantity": 40, "price": 1},
                {"item": "titani", "quantity": 20, "price": 2},
                {"item": "copper", "quantity": 20, "price": 2},
                {"item": "silici", "quantity": 20, "price": 2},
            ],
            [
                {"item": "food", "quantity": 40, "price": 6},
                {"item": "titani", "quantity": 25, "price": 4},
                {"item": "copper", "quantity": 25, "price": 4},
                {"item": "uraniu", "quantity": 10, "price": 8},
                {"item": "silici", "quantity": 20, "price": 3},
                {"item": "terran", "quantity": 12, "price": 50},
            ],
        )
    )
    find_region(anvil.regions, "Ridge").stacks.append(
        un_town(
            100040,
            "Ridge",
            1,
            700,
            10,
            100,
            4,
            "wnplnt",
            6,
            1,
            [("food", 80), ("titani", 12)],
            [{"item": "food", "quantity": 60, "price": 2}],
            [{"item": "titani", "quantity": 10, "price": 4}],
        )
    )
    find_region(anvil.regions, "Isotope").stacks.append(
        un_town(
            100050,
            "Isotope",
            1,
            700,
            10,
            100,
            4,
            "wnplnt",
            4,
            1,
            [("food", 60), ("uraniu", 8)],
            [{"item": "food", "quantity": 60, "price": 2}],
            [{"item": "uraniu", "quantity": 6, "price": 8}],
        )
    )

    rootfast = city("120001", "Rootfast", 12, 3, 4000, 40, 300, [], extra_items=[("terair", 40), ("water", 40)])
    rootfast.children = [
        Stack("120002", "farms", 12, 16, "Rootfast farms"),
        Stack("120003", "cplant", 12, 2, "Rootfast plants"),
        Stack("120004", "inftry", 12, 3, "Rootfast garrison"),
        Stack("120005", "cargob", 12, 1, "Rootfast granary"),
        Stack("120010", "inftry", 12, 3, "Rootfast raid"),
    ]
    rootfast.children[0].upkeep = [("cash", 480)]
    rootfast.children[1].items = [("carbon", 20)]
    rootfast.children[1].upkeep = [("cash", 80)]
    rootfast.children[2].upkeep = [("cash", 45)]
    rootfast.children[3].items = [("food", 800), ("terair", 40), ("water", 40)]
    rootfast.children[3].upkeep = [("cash", 10)]
    rootfast.children[4].upkeep = [("cash", 45)]
    find_region(arbor.regions, "Farm Belt").stacks.append(rootfast)

    crusthold = city("130001", "Crusthold", 13, 2, 5000, 30, 200, [])
    crusthold.children = [
        Stack("130002", "cdrill", 13, 4, "Crusthold drills"),
        Stack("130003", "factry", 13, 2, "Crusthold factories"),
        Stack("130004", "wnplnt", 13, 4, "Crusthold wind"),
        Stack("130005", "inftry", 13, 2, "Crusthold garrison"),
        Stack("130006", "tanks", 13, 1, "Crusthold tanks"),
        Stack("130007", "cargob", 13, 1, "Crusthold granary"),
        Stack("130010", "inftry", 13, 3, "Crusthold raid"),
    ]
    crusthold.children[0].upkeep = [("cash", 200)]
    crusthold.children[1].upkeep = [("cash", 110)]
    crusthold.children[2].upkeep = [("cash", 4)]
    crusthold.children[3].upkeep = [("cash", 30)]
    crusthold.children[4].upkeep = [("cash", 80)]
    crusthold.children[5].items = [("food", 120), ("iron", 20), ("titani", 15), ("silici", 15)]
    crusthold.children[5].upkeep = [("cash", 10)]
    crusthold.children[6].upkeep = [("cash", 45)]
    find_region(anvil.regions, "Vale").stacks.append(crusthold)

    player_home = {
        2: ("Northwind Grant", "arbor"),
        3: ("Greenwell Grant", "arbor"),
        4: ("Rivermark Grant", "arbor"),
        5: ("Sundock Grant", "arbor"),
        6: ("Copse Grant", "arbor"),
        7: ("Ironclad Grant", "anvil"),
        8: ("Oreline Grant", "anvil"),
        9: ("Basalt Grant", "anvil"),
        10: ("Silicate Grant", "anvil"),
        11: ("Fission Grant", "anvil"),
    }
    for fac, (grant, home) in player_home.items():
        grid = arbor.regions if home == "arbor" else anvil.regions
        find_region(grid, grant).stacks.append(hq_stack(fac, home))

    wreck_h = Stack("W00001", "alnhul", 1, 1, "Helios belt hulk")
    wreck_h.upkeep = [("cash", 100)]
    landings["helios-belt"].stacks.append(wreck_h)
    wreck_f = Stack("W00002", "robofc", 1, 1, "Fomal belt fabricator")
    wreck_f.upkeep = [("cash", 30)]
    landings["fomal-belt"].stacks.append(wreck_f)

    ids.skip_planets_to(11)
    # Empty systems SS0003–SS0010 (signature ores; moons ≥10; Graph 6×6).
    # Gates for those systems are added after the bodies (ids P00041+).
    # Habitable systems stay catalog M4 (Helios/Fomal default; Deep/Graph here).
    empty = [
        ("SS0003", "Ember", 4, "K2", ember_bodies),
        ("SS0004", "Gleam", 5, "M1", gleam_bodies),
        ("SS0005", "Cinder", 6, "K5", cinder_bodies),
        ("SS0006", "Ash", 7, "M0", ash_bodies),
        ("SS0007", "Shards", 8, "G8", shards_bodies),
        ("SS0008", "Deep", 9, "M4", deep_bodies),
        ("SS0009", "Graph", 10, "M4", graph_bodies),
        ("SS0010", "Spare", 11, "K3", spare_bodies),
    ]
    for ss, sname, x, typ, builder in empty:
        system = System(ss, sname, x, 0, 0)
        system.star = Star(ids.S(), sname, typ=typ)
        builder(ids, system)
        systems.append(system)

    add_empty_system_gates(ids, systems)
    return systems, landings


def add_empty_system_gates(ids, systems):
    """One Gate per empty system, paired 1:1 with a homeworld outbound Gate.

    Helios (Arbor) opens Ember, Cinder, Ash, Graph.
    Fomal (Anvil) opens Gleam, Shards, Deep, Spare.
    Existing Helios Fomal Gate P00009 remains paired only with Fomal Helios Gate P00010.
    """
    by_name = {s.name_en: s for s in systems}
    ids.skip_planets_to(41)
    links = (
        ("Helios", "Ember"),
        ("Helios", "Cinder"),
        ("Helios", "Ash"),
        ("Helios", "Graph"),
        ("Fomal", "Gleam"),
        ("Fomal", "Shards"),
        ("Fomal", "Deep"),
        ("Fomal", "Spare"),
    )
    for home_en, empty_en in links:
        home_id = ids.P()
        empty_id = ids.P()
        home_gate = Alderson(home_id, "%s %s Gate" % (home_en, empty_en), 80, empty_id)
        home_gate.orbit = Orbit(ids.O())
        empty_gate = Alderson(empty_id, "%s %s Gate" % (empty_en, home_en), 80, home_id)
        empty_gate.orbit = Orbit(ids.O())
        by_name[home_en].aldersons.append(home_gate)
        by_name[empty_en].aldersons.append(empty_gate)


def add_belt(ids, system, name_en, au, composition):
    belt = Belt(ids.P(), name_en, au, composition)
    system.belts.append(belt)
    return belt


def add_ring(ids, planet, name_en, au, composition):
    ring = Belt(ids.P(), name_en, format_moon_au(au), composition)
    planet.belts.append(ring)
    return ring


# Orbit-held cloud isotopes by atmosphere band (designer/environments.md, resources.md).
GAS_CLOUD_BY_NAME = {
    "Aeolus": (100, 70),
    "Fomal Giant": (90, 65),
    "Ember Giant": (110, 75),
    "Cinder Giant": (85, 90),
    "Ash Inner Giant": (95, 80),
    "Ash Outer Giant": (70, 95),
    "Deep Inner Giant": (105, 70),
    "Deep Outer Giant": (80, 100),
}


def gas_cloud_resources(atmosphere, name_en=""):
    if not atmosphere or atmosphere == "none":
        return []
    if atmosphere == "thin":
        return [("heliu3", 40)]
    if atmosphere == "terair":
        return [("heliu3", 30)]
    if atmosphere == "hostile":
        he3, d2 = GAS_CLOUD_BY_NAME.get(name_en, (100, 80))
        return [("heliu3", he3), ("deutrm", d2)]
    return [("heliu3", 60)]


def add_planet(ids, system, name_en, typ, au, w, h, env, typer, res_fn, cap_fn=None, moons=None, races=None):
    gravity, temperature, atmosphere = env
    planet = Planet(ids.P(), name_en, typ, au, w, h, gravity, temperature, atmosphere)
    orbit_resources = gas_cloud_resources(atmosphere, name_en) if typ == "gasgnt" else None
    planet.orbit = Orbit(ids.O(), resources=orbit_resources)
    if races:
        planet.races = list(races)
    if w > 0 and h > 0:
        planet.regions = make_pocket_grid(
            ids, w, h, lambda x, y: "%s %d,%d" % (name_en, x, y), typer, res_fn, cap_fn
        )
    for spec in moons or []:
        mname, mtyp, mau, mw, mh, menv, mtyp_fn, mres = spec[:8]
        mraces = None
        mcap = None
        for extra in spec[8:]:
            if callable(extra):
                mcap = extra
            elif extra:
                mraces = list(extra)
        moon = Moon(ids.M(), mname, mtyp, mau, mw, mh, *menv)
        moon.orbit = Orbit(ids.O())
        if mraces:
            moon.races = list(mraces)
        moon.regions = make_pocket_grid(
            ids, mw, mh, lambda x, y, mname=mname: "%s %d,%d" % (mname, x, y), mtyp_fn, mres, mcap
        )
        planet.moons.append(moon)
    system.planets.append(planet)
    return planet


def ember_bodies(ids, system):
    add_planet(
        ids,
        system,
        "Ember Dust",
        "dust",
        0.7,
        5,
        3,
        ("low", "hot", "thin"),
        lambda x, y: "dust" if y < 2 else "barren",
        lambda x, y, typ: [("titani", 20), ("silici", 18)]
        + ([("lithia", 60)] if typ == "barren" and x in (0, 2) else []),
        lambda x, y, typ: [("extraction", 3)],
    )
    add_belt(
        ids,
        system,
        "Ember Belt",
        2.2,
        [("iron", 15, 0.5), ("silici", 12, 0.5), ("heliu3", 16, 0.2)],
    )
    add_planet(
        ids,
        system,
        "Ember Giant",
        "gasgnt",
        4.1,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
        moons=[
            (
                "Ember Ice %d" % i,
                "ice",
                0.004 + i * 0.002,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ, i=i: [("water", 50), ("heliu3", 20 + (10 if i == 0 else 0))]
                + ([("deutrm", 60)] if i == 0 and x + y == 0 else [])
                + ([("lithia", 20)] if i == 1 and x == 0 and y == 0 else []),
            )
            for i in range(3)
        ],
    )


def gleam_bodies(ids, system):
    add_planet(
        ids,
        system,
        "Gleam Inner",
        "dust",
        0.12,
        4,
        3,
        ("low", "hot", "none"),
        lambda x, y: "barren" if y == 0 else "dust",
        lambda x, y, typ: [("nickfe", 18), ("copper", 14), ("uraniu", 8)]
        + ([("reeox", 50)] if x == 0 and y == 0 else [])
        + ([("platnm", 6)] if x == 1 and y == 0 else []),
        lambda x, y, typ: [("extraction", 3)],
    )
    add_belt(
        ids,
        system,
        "Gleam Belt",
        0.4,
        [
            ("nickfe", 25, 0.6),
            ("uraniu", 12, 0.4),
            ("copper", 10, 0.4),
            ("gold", 8, 0.1),
            ("reeox", 50, 0.3),
            ("platnm", 6, 0.1),
        ],
    )
    add_planet(
        ids,
        system,
        "Gleam Ice",
        "dust",
        0.8,
        4,
        3,
        ("low", "cold", "none"),
        lambda x, y: "dust",
        lambda x, y, typ: [("water", 20), ("nickfe", 10), ("copper", 8)],
    )


def cinder_bodies(ids, system):
    add_planet(
        ids,
        system,
        "Cinder Dust",
        "dust",
        1.2,
        5,
        3,
        ("low", "hot", "thin"),
        lambda x, y: "dust" if x < 3 else "barren",
        lambda x, y, typ: [("carbon", 20), ("uraniu", 10), ("iron", 4)]
        + ([("grphit", 45)] if typ == "barren" and x == 4 and y == 0 else []),
        lambda x, y, typ: [("extraction", 3)],
    )
    add_belt(
        ids,
        system,
        "Cinder Belt",
        2.8,
        [("carbon", 22, 0.6), ("uraniu", 8, 0.4), ("grphit", 20, 0.2)],
    )
    cinder_giant = add_planet(
        ids,
        system,
        "Cinder Giant",
        "gasgnt",
        8.0,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
        moons=[
            (
                "Cinder Vulcan",
                "vulcan",
                0.003,
                5,
                2,
                ("low", "hot", "none"),
                lambda x, y: "mountn",
                lambda x, y, typ: [("carbon", 10), ("iron", 6), ("silici", 20), ("tungst", 8)]
                + ([("boron", 60)] if x + y == 0 else [])
                + ([("grphit", 50)] if x == 1 and y == 0 else []),
            ),
            (
                "Cinder Rock",
                "rock",
                0.005,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("silici", 20), ("iron", 12)],
            ),
            (
                "Cinder Ice",
                "ice",
                0.007,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("water", 40), ("heliu3", 10)],
            ),
        ],
    )
    add_ring(
        ids,
        cinder_giant,
        "Cinder Ring",
        0.002,
        [("carbon", 20, 0.4), ("silici", 15, 0.4), ("iron", 8, 0.3)],
    )


def ash_bodies(ids, system):
    add_belt(
        ids,
        system,
        "Ash Belt",
        3.0,
        [("carbon", 18, 0.5), ("heliu3", 10, 0.3), ("volatl", 14, 0.3)],
    )
    inner = add_planet(
        ids,
        system,
        "Ash Inner Giant",
        "gasgnt",
        6.0,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
        moons=[
            (
                "Ash Moon %d" % i,
                ("ice" if i < 2 else ("rock" if i == 2 else "vulcan")),
                0.003 + i * 0.002,
                5,
                2,
                ("low", "hot" if i == 3 else "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ, i=i: (
                    [("water", 40), ("heliu3", 40 if i == 0 and x + y == 0 else 15), ("ammoni", 16)]
                    + ([("xenon", 50)] if i == 0 and x + y == 0 else [])
                    + ([("deutrm", 50)] if i == 1 and x + y == 0 else [])
                    if i < 2
                    else ([("silici", 16), ("iron", 10)] if i == 2 else [("iron", 5), ("silici", 12)])
                ),
            )
            for i in range(4)
        ],
    )
    add_ring(
        ids,
        inner,
        "Ash Ring",
        0.002,
        [("water", 25, 0.4), ("heliu3", 12, 0.3)],
    )
    add_planet(
        ids,
        system,
        "Ash Outer Giant",
        "gasgnt",
        18.0,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
    )
    return inner


def shards_bodies(ids, system):
    add_belt(
        ids,
        system,
        "Shards Inner Belt",
        2.0,
        [("carbon", 40, 0.6), ("oil", 8, 0.3), ("kerogn", 16, 0.4), ("volatl", 12, 0.3)],
    )
    add_belt(
        ids,
        system,
        "Shards Outer Belt",
        3.1,
        [("carbon", 18, 0.5), ("water", 8, 0.3)],
    )
    add_planet(
        ids,
        system,
        "Shards Dust",
        "dust",
        0.9,
        5,
        3,
        ("low", "cold", "thin"),
        lambda x, y: "dust" if y < 2 else "barren",
        lambda x, y, typ: [("carbon", 12), ("silici", 16)]
        + ([("nitrat", 50)] if typ == "barren" and x in (0, 2) else []),
    )


def deep_bodies(ids, system):
    add_planet(
        ids,
        system,
        "Deep Dust",
        "dust",
        0.5,
        4,
        3,
        ("low", "hot", "thin"),
        lambda x, y: "dust",
        lambda x, y, typ: [("iron", 16), ("silici", 14)],
        lambda x, y, typ: [("extraction", 2)],
    )
    hab_moon = (
        "Haven",
        "ice",
        0.004,
        4,
        3,
        ("low", "habitable", "terair"),
        lambda x, y: "grassl" if y == 1 else ("sea" if y == 2 else "dust"),
        lambda x, y, typ: (
            [("food", 40), ("water", 60)] if typ in ("grassl", "sea") else [("water", 40)]
        ),
        ["terran"],
        lambda x, y, typ: [("settlement", 8)] if typ == "grassl" else [],
    )
    add_planet(
        ids,
        system,
        "Deep Inner Giant",
        "gasgnt",
        4.5,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
        moons=[
            hab_moon,
            (
                "Deep Ice A",
                "ice",
                0.006,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("water", 50), ("heliu3", 20), ("methn", 16)],
            ),
            (
                "Deep Rock",
                "rock",
                0.008,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("silici", 16), ("iron", 10), ("alumin", 14)],
            ),
            (
                "Deep Ice B",
                "ice",
                0.01,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("water", 45), ("heliu3", 15), ("methn", 12)],
            ),
        ],
    )
    add_planet(
        ids,
        system,
        "Deep Outer Giant",
        "gasgnt",
        9.2,
        0,
        0,
        ("high", "cold", "hostile"),
        lambda x, y: "dust",
        lambda x, y, typ: [],
        moons=[
            (
                "Deep Outer %d" % i,
                "ice" if i else "rock",
                0.004 + i * 0.003,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ, i=i: (
                    [("water", 30), ("heliu3", 10), ("methn", 10)]
                    if i
                    else [("iron", 12), ("silici", 10), ("alumin", 12)]
                ),
            )
            for i in range(3)
        ],
    )


def graph_bodies(ids, system):
    graph_types = {
        (0, 0): "ocean",
        (1, 0): "sea",
        (2, 0): "grassl",
        (3, 0): "grassl",
        (4, 0): "dust",
        (5, 0): "mountn",
        (0, 1): "ocean",
        (1, 1): "sea",
        (2, 1): "grassl",
        (3, 1): "grassl",
        (4, 1): "grassl",
        (5, 1): "dust",
        (0, 2): "ocean",
        (1, 2): "grassl",
        (2, 2): "grassl",
        (3, 2): "grassl",
        (4, 2): "mountn",
        (5, 2): "dust",
        (0, 3): "sea",
        (1, 3): "grassl",
        (2, 3): "grassl",
        (3, 3): "dust",
        (4, 3): "mountn",
        (5, 3): "barren",
        (0, 4): "ocean",
        (1, 4): "sea",
        (2, 4): "grassl",
        (3, 4): "dust",
        (4, 4): "barren",
        (5, 4): "mountn",
        (0, 5): "ocean",
        (1, 5): "ocean",
        (2, 5): "sea",
        (3, 5): "grassl",
        (4, 5): "dust",
        (5, 5): "barren",
    }

    def graph_type(x, y):
        return graph_types[(x, y)]

    def graph_res(x, y, typ):
        if typ in ("ocean", "sea"):
            return [("water", 400), ("food", 20)]
        if typ == "grassl":
            return [("food", 300), ("carbon", 20), ("water", 80), ("iron", 12)]
        if typ == "mountn":
            return [("silici", 22), ("iron", 18), ("alumin", 14)] + (
                [("gold", 6)] if x == 4 and y == 2 else []
            )
        if typ == "barren":
            return [("silici", 18), ("alumin", 12)]
        return [("silici", 20), ("iron", 15), ("carbon", 8)]

    def graph_cap(x, y, typ):
        if typ == "grassl":
            return [("settlement", 8)]
        return []

    ocean = add_planet(
        ids,
        system,
        "Graph",
        "ocean",
        0.95,
        6,
        6,
        ("normal", "habitable", "terair"),
        graph_type,
        graph_res,
        graph_cap,
        races=["terran"],
    )
    add_planet(
        ids,
        system,
        "Graph Dust",
        "dust",
        1.6,
        5,
        4,
        ("low", "cold", "none"),
        lambda x, y: "dust" if y < 3 else "mountn",
        lambda x, y, typ: [("silici", 18), ("iron", 12), ("carbon", 8), ("alumin", 10)],
        moons=[
            (
                "Graph Rock",
                "rock",
                0.002,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("silici", 14), ("iron", 10), ("alumin", 12)],
            )
        ],
    )
    add_belt(
        ids,
        system,
        "Graph Belt",
        2.5,
        [("carbon", 20, 0.5), ("iron", 10, 0.4), ("silici", 10, 0.4)],
    )
    return ocean


def spare_bodies(ids, system):
    add_planet(
        ids,
        system,
        "Spare Dust",
        "dust",
        1.1,
        5,
        3,
        ("low", "cold", "none"),
        lambda x, y: "dust" if y < 2 else "mountn",
        lambda x, y, typ: [("titani", 45 if x + y == 0 else 22), ("copper", 16)]
        + ([("berylm", 55)] if typ == "mountn" and x in (0, 2) else []),
        lambda x, y, typ: [("extraction", 3)],
        moons=[
            (
                "Spare Ice",
                "ice",
                0.003,
                5,
                2,
                ("low", "cold", "none"),
                lambda x, y: "dust",
                lambda x, y, typ: [("water", 40)],
            )
        ],
    )
    add_belt(
        ids,
        system,
        "Spare Belt",
        2.4,
        [("titani", 14, 0.5), ("copper", 12, 0.4), ("iron", 10, 0.4)],
    )


def emit_factions(root):
    el(
        root,
        "faction",
        name="1",
        **{"name-en": "United Star Nations"},
        password="",
        email="",
        **{"default-attitude": "2"},
        **{"text-report": "True"},
        **{"text-report-line-length": "100"},
        **{"xml-report": "True"},
        balance="0",
        **{"credit-line": "0"},
        **{"credit-rate": "0"},
        **{"deposit-rate": "0"},
    )
    for fac, (name_en, password) in PLAYERS.items():
        el(
            root,
            "faction",
            name=str(fac),
            **{"name-en": name_en},
            password=password,
            email="player%d@local" % fac,
            **{"default-attitude": "2"},
            **{"text-report": "True"},
            **{"text-report-line-length": "100"},
            **{"xml-report": "True"},
            balance="10000",
            **{"credit-line": "10000"},
            **{"credit-rate": "0.2"},
            **{"deposit-rate": "0.05"},
        )
    el(
        root,
        "faction",
        name="12",
        **{"name-en": "Arbor First"},
        password="",
        email="",
        **{"default-attitude": "2"},
        **{"text-report": "True"},
        **{"text-report-line-length": "100"},
        **{"xml-report": "True"},
        balance="4000",
        **{"credit-line": "0"},
        **{"credit-rate": "0"},
        **{"deposit-rate": "0"},
    )
    el(
        root,
        "faction",
        name="13",
        **{"name-en": "HCS"},
        password="",
        email="",
        **{"default-attitude": "2"},
        **{"text-report": "True"},
        **{"text-report-line-length": "100"},
        **{"xml-report": "True"},
        balance="5000",
        **{"credit-line": "0"},
        **{"credit-rate": "0"},
        **{"deposit-rate": "0"},
    )


def emit_contracts(root, landings):
    contracts = el(root, "contracts")
    el(
        contracts,
        "contract",
        name="CT0001",
        location="R00040",
        issuer="1",
        trigger="give-module",
        **{"reward-type": "technology"},
        reward="ctypln",
        quantity="1",
        module="farms",
        receiver="100030",
        baseline="6",
        title="Slagport calorie charter",
        flavour="Slagport's granary spectra show a thin 1.6 um water-ice band and a weak chlorophyll shoulder. Deliver one farming complex so the metal town can close its own calorie loop.",
    )
    el(
        contracts,
        "contract",
        name="CT0002",
        location="R00068",
        issuer="1",
        trigger="give-module",
        **{"reward-type": "technology"},
        reward="servic",
        quantity="2",
        module="wnplnt",
        receiver="100050",
        baseline="4",
        title="Isotope wind survey",
        flavour="Isotope sits on a uraninite spine with a measured 8 m/s mean surface wind. Two more wind plants keep the assay mill online without burning the local fissile stockpile.",
    )
    el(
        contracts,
        "contract",
        name="CT0003",
        location="R00003",
        issuer="1",
        trigger="give-module",
        **{"reward-type": "technology"},
        reward="airgen",
        quantity="1",
        module="cdrill",
        receiver="100010",
        baseline="0",
        title="Tidewatch core sample",
        flavour="Tidewatch Coast oil seeps sit above a granite basement. A core drill lets UN assay the sediment column without importing Anvil crust kits.",
    )
    el(
        contracts,
        "contract",
        name="CT0004",
        location=landings["cinder"].name,
        issuer="1",
        trigger="research",
        **{"reward-type": "technology"},
        reward="ahlcns",
        target="W00001",
        points="24",
        title="Helios belt hull charter",
        flavour="Radar returns from the metal landing rock match a high-albedo, crewless hull geometry. Research the hulk in situ; delta-v from Arbor is a belt-class chemical hop.",
    )
    el(
        contracts,
        "contract",
        name="CT0005",
        location=landings["pad"].name,
        issuer="1",
        trigger="research",
        **{"reward-type": "technology"},
        reward="advres",
        target="W00002",
        points="24",
        title="Fomal carbonaceous fabricator",
        flavour="A cold, unmanned fabrication plant is wedged in a kerogen-rich carbonaceous cell. Research it on site; the organics are feedstock, not a skip of the Arbor/Anvil split.",
    )


def emit_galaxy(root, systems):
    galaxy = el(root, "galaxy")
    for system in systems:
        sys_el = el(
            galaxy,
            "system",
            name=system.name,
            **{"name-en": system.name_en},
            X=system.x,
            Y=system.y,
            Z=system.z,
        )
        el(
            sys_el,
            "star",
            **with_description(
                {"name": system.star.name, "name-en": system.star.name_en, "type": system.star.typ},
                system.star,
            )
        )
        for planet in system.planets:
            emit_planet(sys_el, planet)
        for belt in system.belts:
            emit_belt(sys_el, belt)
        for alderson in system.aldersons:
            emit_alderson(sys_el, alderson)


def collect_stacks(systems):
    found = []

    def walk_stack(stack):
        found.append(stack)
        for child in stack.children:
            walk_stack(child)

    def walk_regions(regions):
        for region in regions:
            for stack in region.stacks:
                walk_stack(stack)

    for system in systems:
        for planet in system.planets:
            walk_regions(planet.regions)
            if planet.orbit:
                for stack in planet.orbit.stacks:
                    walk_stack(stack)
            for moon in planet.moons:
                walk_regions(moon.regions)
                if moon.orbit:
                    for stack in moon.orbit.stacks:
                        walk_stack(stack)
            for ring in planet.belts:
                for stack in ring.stacks:
                    walk_stack(stack)
        for belt in system.belts:
            for stack in belt.stacks:
                walk_stack(stack)
        for alderson in system.aldersons:
            if alderson.orbit:
                for stack in alderson.orbit.stacks:
                    walk_stack(stack)
            for stack in alderson.stacks:
                walk_stack(stack)
    return found


STAR_FLAVOUR = {
    "Helios": (
        "The primary is a coin of warm gold, about the Sun radius, about the Sun luminosity - "
        "the colour of wheat and old brass. Limb darkening turns the edge a softer orange. "
        "Arbor hangs in the one-AU water zone like something you were always meant to see: "
        "white cloud, green basins, the kind of blue that makes a visor feel like a mistake. "
        "The pair-axis is a rumour of Fomal, too far for the eye. You have not come to a wilderness. "
        "You have come home to a lamp that feeds cities."
    ),
    "Fomal": (
        "Hotter gold than the Sun, a shade toward white, still catalog M4, still about the Sun in size and output. "
        "The light is impatient. Anvil at 1.4 AU looks mineral even from the Gate: thinner green, more glare off highland, "
        "a world that grew metals and fissiles instead of peat. There is air enough to breathe and not enough kindness in the soil. "
        "The star does not look cruel. The crust will."
    ),
    "Ember": (
        "A smaller disk than Helios, maybe four-fifths as wide, two-fifths as bright, the colour of a banked forge. "
        "Inner dust is a kiln, too close, too dry. Past the ice line a pale giant holds three cold moons. "
        "The chemistry that matters is not on the baked plains. It is in freeze-worked brines: alkali salts leached from silicate, "
        "waiting in the dark. The star will outlive your corporation. It does not hurry you."
    ),
    "Gleam": (
        "The primary is a red coal, half a solar width, a few percent of a solar glow, so close that noon is a swollen wine-dark disk. "
        "The metal belt rides that glare. A flare can stitch white across the red without warning. "
        "These rocks never finished degassing: nickel-iron and rare-earth oxides still live in the metal phase. "
        "You feel the particle flux in the hull before you feel wonder. Then the wonder arrives anyway - "
        "a furnace that has been waiting since before language."
    ),
    "Cinder": (
        "Copper-orange, seven-tenths of a sun across, maybe a sixth as luminous, smoky at the limb. "
        "Vulcan moons glow in that light as if the star and the rock agreed on a temperature. "
        "Carbon here was cooked past any wetland story into hard lattice. Fumaroles leave borate crust. "
        "There is no green to rest the eye. Arrival is a held breath. The star looks near enough to scorch the Gate and old enough not to notice."
    ),
    "Ash": (
        "The disk is wrong. It is giant-class: tens of solar radii, dull blood-red, lazy light that can still outshine hundreds of Helios-class lamps. "
        "The ice line has been shoved into the outer dark. Warm dust is a lie. Far out, grainy ices hold adsorbed noble gases the way glass holds breath. "
        "The star fills too much of the sky for how cold the prize is. You will travel a long time under that red before you are close to what you came for."
    ),
    "Shards": (
        "Butter-yellow, almost a home star: nine-tenths of a solar radius, four-fifths of a solar luminosity. "
        "The familiarity is a trap. The chromosphere is young; ultraviolet still bites. "
        "Dry pans bleach into evaporite oxidizer salts. Carbonaceous belts keep ice and organics, but nothing here invited a city. "
        "The yellow looks like welcome. The spectrometer disagrees."
    ),
    "Deep": (
        "Catalog M4 on a dimmer orange lamp - still habitable-class, a little smaller in the mind than Helios, gold sliding toward ember. "
        "Haven is the reason the token stayed M4: a thin ribbon of sea and grassland on an ice moon, tight calories, air you can almost trust. "
        "The outer ices keep methane-family volatiles. You feel, arriving, that someone could live here if they were careful and a little hungry."
    ),
    "Graph": (
        "A clean yellow analog, catalog M4, near one solar radius and luminosity, the 0.95 AU ocean world already a bright sickle in the Gate light. "
        "Cloud, water, silica coasts, iron in the highlands - a carbon-and-stone prize, not an industrial signature world. No cities. "
        "The star does not know it is empty. For a minute after JUMP you can pretend the green is spoken for. "
        "Then the silence of the radio makes the pretence expensive."
    ),
    "Spare": (
        "Orange and even, three-quarters of a solar width, a fifth of a solar glow, the colour of a lantern left in a window. "
        "The inner crust is a light-metal leftover: residual melts, impact glass, structural alkali-earths, copper-family ballast. "
        "No air. No farms. The star is not trying to impress you. The loneliness is complete and, after a while, honest. "
        "You came for what the rock refused to become."
    ),
}

BODY_FLAVOUR = {
    "Arbor": (
        "An ocean world under gold light, one AU out, the size of a settled continent mapped and a pelagic rest left off the chart. "
        "White cloud, green basins, coastal peat and banded iron in old sediments. The air is thick enough to forget the visor. "
        "There is no rutile glare here, no pitchblende spine - organics and common iron, and a hinterland that still has empty grass. "
        "UN towns sit like pins in a living map. You could walk without a suit. That is the luxury and the trap."
    ),
    "Selene": (
        "Arbor's rock moon is a pale coin in the gold. Low gravity, vacuum, cold enough that the poles keep ice in the dust. "
        "Highlands show light structural metal and silica; the rest is grey powder that never learned rain. "
        "From orbit the homeworld fills half the sky. Landing feels like stepping onto a loft above a garden you are not ready to leave."
    ),
    "Scoria": (
        "Hot dust at 1.5 AU, thin air, low gravity - a kiln world next door to the garden. "
        "Ilmenite plains and copper-family stains run in the glare. Nothing green. Extraction pads, not towns. "
        "The gold of Helios is harsher here. You come for metals the homeworld will not grow, and you leave with dust in every seal."
    ),
    "Helios Belt": (
        "A dark necklace at 2.7 AU: metal rock and carbonaceous grit sharing the same cold light. "
        "Fissile pockets and nickel-iron hide in the metal phase; the darker stones keep organics and common carbon. "
        "No surface, no air, no kindness. A wreck rumour lives here like a second star - something crewless and bright that should not be."
    ),
    "Aeolus": (
        "A banded gas giant at 5.2 AU, high gravity at the cloud deck, hostile mix, no ground to stand on. "
        "The disk is cream and rust, slow storms the size of worlds. You do not land. You park in the cold and look down. "
        "Four moons and a thin ring do the work of a surface. The giant itself is weather and mass."
    ),
    "Rime": (
        "The innermost ice moon of Aeolus: low, cold, airless, rich water ice and light fusion isotopes in the regolith. "
        "Helios is a distant gold coin. The giant fills the other half of the sky. "
        "You walk on packed frost that never melted. The prize is what the ice kept, not what it looks like."
    ),
    "Glaze": (
        "A second ice: a little farther, a little poorer in isotopes, still a white desert under the giant's shadow. "
        "Water is the reason to land. The rest is silence and a horizon that curves too fast. "
        "From the night side Aeolus is a wall of cream lightning. You feel small on purpose."
    ),
    "Shard": (
        "Aeolus's rock moon: vacuum, cold, silica and iron and light structural metal in the highlands. "
        "No ice to soften the boots. The surface is a broken plate, older than the garden world inward. "
        "You come when the ices are already claimed, or when you want stone that does not lie about being alive."
    ),
    "Cindercone": (
        "A vulcan moon in the same family: hot, low, no air worth naming. Fumaroles stain the highlands. "
        "Carbon here is baked, not grown. The light of Helios is a memory; the heat is local. "
        "Landing is a negotiation with a world that is still cooking."
    ),
    "Aeolus Ring": (
        "A thin ice-and-grit ring hugging the giant. No orbit of your own - you MOVE onto the belt itself. "
        "Water ice and silica dust, cold enough to keep a glove stiff. Beautiful only if you like knives of light."
    ),
    "Anvil": (
        "Habitable and unkind. Breathable air over a thinner biosphere, 1.4 AU from an impatient gold-white lamp. "
        "Shield volcanoes show titanium-family metal, native copper-family veins, fissile spines. Wetlands are scarce. "
        "Food is poor; petroleum never paid a rent. The UN towns look hungry even from orbit. "
        "You can take a helmet off. You will still taste dust and ore."
    ),
    "Anvil Rock": (
        "The rock moon: low, cold, vacuum, metals in the dust and mandatory polar ice so a base can drink. "
        "Anvil hangs huge and mineral-brown below. There is no kindness here, only water in the shade and stone in the sun."
    ),
    "Anvil Ice": (
        "A smaller ice moon, rich water, light isotopes, the colour of old bone. "
        "Fomal's hotter gold makes the terminator a hard line. You came for ice, not for a view, but the view will stay with you."
    ),
    "Pyre": (
        "Inner dust at 0.6 AU, hot, thin air, iron and silica in the glare. Not a place to live. "
        "The star is too large in the sky. Landing is a short season of sweat and extraction, then leave before the seals complain."
    ),
    "Fomal Belt": (
        "Carbonaceous, 2.5 AU out: dark rock, heavy hydrocarbons, the organics Anvil's crust refused to grow. "
        "No metal glitter. The belt looks like soot against the white-gold lamp. A cold fabricator rumour sits in the grit."
    ),
    "Fomal Giant": (
        "A cold gas world at 6 AU, high deck gravity, hostile air, cream belts that do not care you arrived. "
        "Two ice moons and a ring do the mining. The giant is a mass and a weather system, not a destination."
    ),
    "Drift": (
        "An ice moon of the Fomal giant: water, light isotopes, a slow year in the dim. "
        "The primary is a bright point. The giant is a wall. You land for volatiles and leave before the cold writes your name."
    ),
    "Rimeband": (
        "The richer ice of the pair - more fusion-light isotope in the frost, same vacuum, same honesty. "
        "No air. No farms. The ring cuts a line across the giant like a scar you could walk."
    ),
    "Fomal Ring": (
        "Ice grit on a short leash around the giant. Water and silica, no drama except the light. "
        "You occupy the belt, not a landing. It feels like standing on a rumour."
    ),
    "Ember Dust": (
        "Hot inner dust under amber light, too close, too dry. Titanium-family oxides and silica, no green, no brines. "
        "The kiln look is honest. The chemistry you want is not here. This world is the warning before the ice."
    ),
    "Ember Belt": (
        "A thin belt at 2.2 AU: iron, silica, a hint of light isotope in the cold. "
        "Amber from the dwarf makes the rocks look warmer than they are. They are not warm."
    ),
    "Ember Giant": (
        "A pale ice-giant at 4.1 AU, treated as a gas body: no ground, hostile deck, three ice moons. "
        "The orange lamp looks small from here. This is where the brines begin. The giant is a cold throne for them."
    ),
    "Ember Ice 0": (
        "The innermost ice: packed frost, light isotopes, and freeze-worked alkali brines in the dark. "
        "This is the reason Ember exists on a map. The star is a distant forge. You came for what the ice stole from the rock."
    ),
    "Ember Ice 1": (
        "A middle ice, still wet in the mineral sense, still patient. Less isotope flash, more quiet brine chemistry. "
        "The giant's shadow is a regular night. You can work here if you like silence."
    ),
    "Ember Ice 2": (
        "The outer of the three: colder, poorer, still water enough to matter. "
        "You land here when the inner ices are claimed, or when you want to be alone with a pale giant."
    ),
    "Gleam Inner": (
        "A barren-dust world hugging a red coal, 0.12 AU, hot vacuum. Nickel-iron and copper-family metal, fissile traces, rare-earth oxides in the metal phase. "
        "Noon is a swollen wine disk. Flares write white on the hull. Nothing here wanted life. Everything here wanted to stay metal."
    ),
    "Gleam Belt": (
        "The close-in metal belt: the real prize. Nickel-iron, fissiles, copper-family metal, rare-earth oxides, a trace of precious contacts. "
        "The red dwarf is too large. You mine in a glare that feels like standing next to a furnace door."
    ),
    "Gleam Ice": (
        "A colder dust world at 0.8 AU: some water ice, leftover metal, the first place the red light feels distant. "
        "Not a garden. A shade. You drink here. You do not settle."
    ),
    "Cinder Dust": (
        "Hot dust under copper-orange light: carbon that was never peat, fissile traces, iron as an afterthought. "
        "High-temperature baking has already happened. The world looks like a foundry floor left in a vacuum."
    ),
    "Cinder Belt": (
        "Dark carbonaceous rock at 2.8 AU, some fissile glitter, baked carbon in the mix. "
        "The orange dwarf is a coin. The belt does not glow. You do."
    ),
    "Cinder Giant": (
        "A distant gas giant at 8 AU, three moons - vulcan, rock, ice - and a thin ring. "
        "This is the industrial attic of the system. The star is small. The work is not."
    ),
    "Cinder Vulcan": (
        "Hot highland, no air, fumaroles that leave borate crust and carbon cooked into hard lattice. "
        "Refractory metal sits in the vents. There is no green to forgive the heat. Landing is a held breath that never quite lets go."
    ),
    "Cinder Rock": (
        "A cold rock moon: silica and iron, no performance. "
        "You come for ballast and a place to stand that is not on fire. The vulcan next door still lights the sky."
    ),
    "Cinder Ice": (
        "Water ice and a little light isotope, far from the copper lamp. "
        "A drink after the foundry. The giant is a pale stripe. You will remember the quiet more than the ice."
    ),
    "Cinder Ring": (
        "Carbon and silica grit on a short orbit of the giant. Baked, not grown. "
        "You MOVE onto it. It does not welcome you. It does not care."
    ),
    "Ash Belt": (
        "At 3 AU under a swollen red disk: carbonaceous grit, light isotopes, mixed volatiles. "
        "The star is too large for this distance. The belt feels like ash from a fire that has not gone out in a billion years."
    ),
    "Ash Inner Giant": (
        "The working giant at 6 AU: four mixed moons - ice, ice, rock, vulcan - and a ring. "
        "Noble gases wait on the outer ices, not on this deck. The giant is weather. The moons are the map."
    ),
    "Ash Outer Giant": (
        "Farther still, 18 AU, no moons worth a name. A second mass in the dark. "
        "You pass it. You do not stay. The red lamp is still huge and still cold at the edge."
    ),
    "Ash Moon 0": (
        "The first ice: water, ammonia-family frost, light isotopes, and adsorbed noble gas on the grain. "
        "This is why Ash is on the chart. The giant is a blood-red wall. You came a long way under that light for a cold that keeps secrets."
    ),
    "Ash Moon 1": (
        "The second ice: more water, more ammonia-family ice, a pocket of heavy hydrogen in the frost. "
        "Quieter than the first. Still no air. Still the red disk too large in the mind."
    ),
    "Ash Moon 2": (
        "Rock: silica and iron, a place to stand that is not ice. "
        "You land here to rest the drills, not to get rich. The ices next door are the conversation."
    ),
    "Ash Moon 3": (
        "A small vulcan, hot, low, airless. Iron and silica, no borate fame. "
        "A leftover oven. Useful if you like heat. Not why you jumped."
    ),
    "Ash Ring": (
        "Water ice and light isotope grit around the inner giant. "
        "Pretty in the red. Thin. You occupy it like a thought you cannot quite hold."
    ),
    "Shards Inner Belt": (
        "The carbonaceous heart at 2.0 AU: dark rock, heavy hydrocarbons, kerogen-family organics, mixed volatiles. "
        "Young yellow light makes the soot look almost warm. It is not. This is the organics a garden world would have eaten."
    ),
    "Shards Outer Belt": (
        "Farther, icier, still carbon-dark. Water in the mix. "
        "The UV from the young lamp is a sting even here. You mine with the visor down."
    ),
    "Shards Dust": (
        "A cold dust world at 0.9 AU: silica, carbon, and evaporite pans bleached by young ultraviolet. "
        "Oxidizer salts sit in the dry lakes. Nothing invited a city. The yellow star looks like home and is not."
    ),
    "Deep Dust": (
        "Inner hot dust at 0.5 AU: iron and silica, extraction pads, no air worth a farm. "
        "The dimmer gold-orange lamp is already a warning. The living moon is farther out. This is only the doorstep."
    ),
    "Deep Inner Giant": (
        "At 4.5 AU, four moons: Haven, two ices, one rock. The giant is the reason they have a sky. "
        "You did not come for the deck. You came for the moon that learned green."
    ),
    "Deep Outer Giant": (
        "9.2 AU, three more moons - rock then ice then ice. Methane-family volatiles on the frosts. "
        "Colder, quieter, hungrier. Haven is a rumour inward. Out here the system tells the truth."
    ),
    "Haven": (
        "An ice moon that learned a thin sea and a ribbon of grass. Low gravity, habitable cool, breathable mix. "
        "Twelve cells of almost-life: tight calories, air you can almost trust, water in the dust. No cities. "
        "The giant is a pale stripe. The star is a dimmer gold. You feel, landing, that someone could live here if they were careful and a little hungry."
    ),
    "Deep Ice A": (
        "Water ice, light isotopes, methane-family frost. Not Haven. Not kind. "
        "You land for volatiles. The habitable ribbon is a bright lie next door."
    ),
    "Deep Rock": (
        "Stone and light metal in the highlands, vacuum, cold. "
        "Aluminium-family crust, iron, silica. A workshop moon. Haven's green does not reach this far."
    ),
    "Deep Ice B": (
        "Another ice: water, isotopes, methane-family volatiles, a little poorer, a little farther. "
        "The giant still fills the sky. You will not write home about this one. You will still fill the tanks."
    ),
    "Deep Outer 0": (
        "The outer giant's rock moon: iron, silica, light metal. A dry step in a wet system. "
        "Deep feels deeper here. The inner green is gone from the sky."
    ),
    "Deep Outer 1": (
        "Ice and methane-family volatiles on the first outer frost. "
        "You came this far for cold chemistry, not for a view. The view is still a giant and a dim gold lamp."
    ),
    "Deep Outer 2": (
        "The last ice: water, light isotope, more methane-family frost. The edge of the map. "
        "Turn around and the system is a story you already paid for."
    ),
    "Graph": (
        "An ocean world at 0.95 AU under a clean yellow analog: cloud, water, silica coasts, iron in the highlands, a little precious metal in the peaks. "
        "Thirty-six cells of continent and sea, grassland that could take a town, no town on it. "
        "Carbon and stone, not an industrial signature. The star does not know the radio is empty. You will."
    ),
    "Graph Dust": (
        "A colder dust world at 1.6 AU: silica, iron, carbon, light metal, one rock moon. "
        "The ocean prize is inward and blue. This is the attic: dry, honest, useful if you already have air."
    ),
    "Graph Rock": (
        "The dust world's moon: silica, iron, aluminium-family stone, vacuum. "
        "Graph hangs as a marble you could almost drink. You are not here to drink. You are here to cut."
    ),
    "Graph Belt": (
        "Carbon, iron, silica at 2.5 AU. A quiet belt. No wreck rumour yet. "
        "The yellow lamp is still kind from here. The belt does not care."
    ),
    "Spare Dust": (
        "Barren dust and highland under an even orange lantern: titanium-family metal, copper-family ballast, light alkali-earth in the residual melts. "
        "No air. No farms. The loneliness is the geology. You came for what the crust refused to become."
    ),
    "Spare Ice": (
        "A small ice moon, water only, no performance. "
        "A drink after the dust. The orange star looks like a window someone forgot to close."
    ),
    "Spare Belt": (
        "Titanium-family metal, copper-family grit, iron - a thin belt at 2.4 AU. "
        "Spare by name and by feeling. You will not write a song about it. You may still fill a hold."
    ),
}


def apply_flavour(systems):
    missing = []
    for system in systems:
        star = system.star
        text = STAR_FLAVOUR.get(star.name_en)
        if not text:
            missing.append("star %s" % star.name_en)
        else:
            star.description = text
        for planet in system.planets:
            text = BODY_FLAVOUR.get(planet.name_en)
            if not text:
                missing.append("planet %s" % planet.name_en)
            else:
                planet.description = text
            for moon in planet.moons:
                text = BODY_FLAVOUR.get(moon.name_en)
                if not text:
                    missing.append("moon %s" % moon.name_en)
                else:
                    moon.description = text
            for ring in planet.belts:
                text = BODY_FLAVOUR.get(ring.name_en)
                if not text:
                    missing.append("ring %s" % ring.name_en)
                else:
                    ring.description = text
        for belt in system.belts:
            text = BODY_FLAVOUR.get(belt.name_en)
            if not text:
                missing.append("belt %s" % belt.name_en)
            else:
                belt.description = text
    if missing:
        raise SystemExit("missing flavour for: " + ", ".join(missing))


def validate(systems, landings):
    errors = []
    if len(systems) != 10:
        errors.append("expected 10 systems, got %d" % len(systems))
    names = [s.name for s in systems]
    if names[:2] != ["SS0001", "SS0002"]:
        errors.append("start systems: %s" % names[:2])
    planets = [p for s in systems for p in s.planets]
    planet_ids = [p.name for p in planets]
    aldersons = [a for s in systems for a in s.aldersons]
    alderson_ids = [a.name for a in aldersons]
    if "P00009" not in alderson_ids or "P00010" not in alderson_ids:
        errors.append("missing Gates")
    if any(p.name in ("P00009", "P00010") for p in planets):
        errors.append("Gates must be <alderson>, not <planet>")
    if len(planet_ids) != len(set(planet_ids)):
        errors.append("duplicate planet ids")
    if any(p.name == "P00001" for p in planets for q in planets if p is not q and q.name == "P00001"):
        errors.append("duplicate P00001")
    pair_map = {a.name: a.pair for a in aldersons}
    if pair_map.get("P00009") != "P00010" or pair_map.get("P00010") != "P00009":
        errors.append("home pair P00009/P00010")
    if len(aldersons) != 18:
        errors.append("expected 18 Gates, got %d" % len(aldersons))
    if any(a.pair not in alderson_ids for a in aldersons):
        errors.append("Gate pair missing")
    if any(pair_map.get(a.pair) != a.name for a in aldersons):
        errors.append("Gate pairs not mutual")
    if len(set(a.pair for a in aldersons)) != len(aldersons):
        errors.append("duplicate Gate pairs")
    body_ids = planet_ids + [b.name for s in systems for b in s.belts]
    body_ids += [r.name for s in systems for p in s.planets for r in p.belts]
    if set(body_ids) & set(alderson_ids):
        errors.append("planet/gate id collision")
    if any(a.orbit is None for a in aldersons):
        errors.append("Gate missing orbit")
    here_by_gate = {}
    for system in systems:
        for gate in system.aldersons:
            here_by_gate[gate.name] = system.name_en
    for system in systems:
        for gate in system.aldersons:
            other = here_by_gate.get(gate.pair)
            expected = "%s %s Gate" % (system.name_en, other)
            if gate.name_en != expected:
                errors.append("Gate name-en %s expected %s" % (gate.name_en, expected))
            if getattr(gate, "description", None):
                errors.append("%s must have no description" % gate.name)
    stacks = collect_stacks(systems)
    by_name = {s.name: s for s in stacks}
    if "120001" not in by_name or by_name["120001"].typ != "city":
        errors.append("missing Rootfast 120001")
    if "130001" not in by_name or by_name["130001"].typ != "city":
        errors.append("missing Crusthold 130001")
    arbor_regions = systems[0].planets[0].regions
    anvil_regions = systems[1].planets[0].regions
    farm_belt = [r for r in arbor_regions if r.name == "R00014"]
    vale = [r for r in anvil_regions if r.name == "R00060"]
    if not farm_belt or not any(s.name == "120001" for s in farm_belt[0].stacks):
        errors.append("Rootfast 120001 must sit on R00014")
    if not vale or not any(s.name == "130001" for s in vale[0].stacks):
        errors.append("Crusthold 130001 must sit on R00060")
    hqs = [s for s in stacks if s.typ == "corphq"]
    if len(hqs) != 10:
        errors.append("expected 10 corphq, got %d" % len(hqs))
    factions = sorted({int(s.faction) for s in hqs})
    if factions != list(range(2, 12)):
        errors.append("HQ factions %s" % factions)
    if landings["cinder"].name != "R00006":
        errors.append("Cinder Flats id %s" % landings["cinder"].name)
    if landings["helios-gate"].name != "P00009":
        errors.append("Helios Gate id")
    if landings["fomal-gate"].name != "P00010":
        errors.append("Fomal Gate id")
    for system in systems:
        for planet in system.planets:
            for region in planet.regions:
                if any(ex.kind == "alderson" for ex in region.exits):
                    errors.append("%s must not exit to an alderson" % region.name)
            for moon in planet.moons:
                for region in moon.regions:
                    if any(ex.kind == "alderson" for ex in region.exits):
                        errors.append("%s must not exit to an alderson" % region.name)
        for alderson in system.aldersons:
            if alderson.exits:
                errors.append("%s must not have region exits" % alderson.name)
    arbor = systems[0].planets[0]
    anvil = systems[1].planets[0]
    if arbor.races != ["terran"] or (arbor.orbit and arbor.orbit.races):
        errors.append("Arbor race must sit on the planet, not orbit")
    if anvil.races != ["terran"] or (anvil.orbit and anvil.orbit.races):
        errors.append("Anvil race must sit on the planet, not orbit")
    empty_systems = systems[2:]
    if [s.name for s in empty_systems] != [
        "SS0003",
        "SS0004",
        "SS0005",
        "SS0006",
        "SS0007",
        "SS0008",
        "SS0009",
        "SS0010",
    ]:
        errors.append("empty systems: %s" % [s.name for s in empty_systems])
    if any(len(s.aldersons) != 1 for s in empty_systems):
        errors.append("each empty system must have exactly one Gate")
    if len(systems[0].aldersons) != 5 or len(systems[1].aldersons) != 5:
        errors.append("each home system must have 5 Gates")
    empty_stacks = [st for s in empty_systems for st in collect_stacks([s])]
    if any(st.typ in ("corphq", "city") for st in empty_stacks):
        errors.append("empty systems must have no HQ or city")
    if any(st.name.startswith("W") for st in empty_stacks):
        errors.append("empty systems must have no t=1 wrecks")

    def body_resources(system):
        types = set()
        for planet in system.planets:
            for region in planet.regions:
                types.update(t for t, _ in region.resources)
            for moon in planet.moons:
                for region in moon.regions:
                    types.update(t for t, _ in region.resources)
            for ring in planet.belts:
                types.update(t for t, _, _ in ring.composition)
        for belt in system.belts:
            types.update(t for t, _, _ in belt.composition)
        return types

    signatures = {
        "SS0003": "lithia",
        "SS0004": "reeox",
        "SS0005": "boron",
        "SS0006": "xenon",
        "SS0007": "nitrat",
        "SS0010": "berylm",
    }
    extra_ores = {
        "SS0003": ("deutrm",),
        "SS0004": ("platnm", "nickfe"),
        "SS0005": ("tungst", "grphit"),
        "SS0006": ("ammoni", "volatl", "deutrm"),
        "SS0007": ("kerogn", "volatl"),
        "SS0008": ("methn", "alumin"),
        "SS0009": ("gold", "alumin"),
        "SS0010": ("titani", "copper"),
    }
    by_name = {s.name: s for s in empty_systems}
    for sid, ore in signatures.items():
        if ore not in body_resources(by_name[sid]):
            errors.append("%s missing signature %s" % (sid, ore))
    for sid, ores in extra_ores.items():
        have = body_resources(by_name[sid])
        missing = [ore for ore in ores if ore not in have]
        if missing:
            errors.append("%s missing ores %s" % (sid, ",".join(missing)))

    graph = next(p for p in by_name["SS0009"].planets if p.name_en == "Graph")
    if graph.races != ["terran"] or len(graph.regions) < 30:
        errors.append("Graph must be terran habitable with 30+ regions, got %d" % len(graph.regions))
    haven = None
    for planet in by_name["SS0008"].planets:
        for moon in planet.moons:
            if moon.name_en == "Haven":
                haven = moon
    if haven is None or haven.races != ["terran"] or len(haven.regions) != 12:
        errors.append("Haven must be terran with 12 regions")
    if haven is not None and haven.atmosphere != "terair":
        errors.append("Haven atmosphere must be terair")
    for system in empty_systems:
        for planet in system.planets:
            for moon in planet.moons:
                if len(moon.regions) < 10:
                    errors.append("%s has %d regions (need >=10)" % (moon.name_en, len(moon.regions)))
            if planet.sx > 0 and planet.sy > 0 and len(planet.regions) < 10:
                errors.append("%s has %d regions (need >=10)" % (planet.name_en, len(planet.regions)))
    if any(p.typ == "abelt" for p in planets):
        errors.append("abelt planets remain; belts must be <belt>")
    if any(p.typ == "adpnt" for p in planets):
        errors.append("adpnt planets remain; gates must be <alderson>")
    helios_belts = [b.name for b in systems[0].belts]
    fomal_belts = [b.name for b in systems[1].belts]
    if "P00003" not in helios_belts:
        errors.append("missing Helios Belt P00003")
    if "P00007" not in fomal_belts:
        errors.append("missing Fomal Belt P00007")
    rings = [ring for s in systems for p in s.planets for ring in p.belts]
    if len(rings) < 3:
        errors.append("expected rings on about half of gas giants, got %d" % len(rings))
    # uniqueness
    region_ids = []
    for system in systems:
        for planet in system.planets:
            region_ids.extend(r.name for r in planet.regions)
            for moon in planet.moons:
                region_ids.extend(r.name for r in moon.regions)
    if "R01490" in region_ids or "R01491" in region_ids:
        errors.append("Gate corona regions remain")
    dup = [r for r in region_ids if region_ids.count(r) > 1]
    if dup:
        errors.append("duplicate regions %s" % sorted(set(dup))[:8])
    stack_ids = [s.name for s in stacks]
    dup_s = [s for s in stack_ids if stack_ids.count(s) > 1]
    if dup_s:
        errors.append("duplicate stacks %s" % sorted(set(dup_s))[:8])
    for s in stacks:
        if len(s.name) > 6:
            errors.append("stack id too long: %s" % s.name)
    for system in systems:
        if not system.star.description:
            errors.append("star %s missing description" % system.star.name_en)
        for planet in system.planets:
            if not planet.description:
                errors.append("planet %s missing description" % planet.name_en)
            for moon in planet.moons:
                if not moon.description:
                    errors.append("moon %s missing description" % moon.name_en)
            for ring in planet.belts:
                if not ring.description:
                    errors.append("ring %s missing description" % ring.name_en)
        for belt in system.belts:
            if not belt.description:
                errors.append("belt %s missing description" % belt.name_en)
    if errors:
        raise SystemExit("validation failed:\n  " + "\n  ".join(errors))


def main():
    systems, landings = build_world()
    apply_flavour(systems)
    validate(systems, landings)
    root = ET.Element("game", turn="1")
    emit_factions(root)
    emit_contracts(root, landings)
    emit_galaxy(root, systems)
    el(root, "orders")
    comment = ET.Comment(" Generated by campaign/_gen_gamein.py from designer/galaxy.md. Do not edit by hand. ")
    root.insert(0, comment)
    if hasattr(ET, "indent"):
        ET.indent(root, space="\t")
    tree = ET.ElementTree(root)
    tree.write(OUT_PATH, encoding="windows-1251", xml_declaration=True)
    size = os.path.getsize(OUT_PATH)
    print("wrote %s (%d bytes, %d systems)" % (OUT_PATH, size, len(systems)))


if __name__ == "__main__":
    sys.exit(main())
