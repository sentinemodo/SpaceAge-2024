/** Player factions 2–11 from play/campaign/gamein.xml and play/designer/galaxy.md */

export type CampaignFaction = {
  id: number;
  name: string;
  planet: string;
};

export const campaignFactionsByPlanet: { planet: string; factions: CampaignFaction[] }[] = [
  {
    planet: 'Arbor (Helios)',
    factions: [
      { id: 2, name: 'Northwind', planet: 'Arbor' },
      { id: 3, name: 'Greenwell', planet: 'Arbor' },
      { id: 4, name: 'Rivermark', planet: 'Arbor' },
      { id: 5, name: 'Sundock', planet: 'Arbor' },
      { id: 6, name: 'Copse', planet: 'Arbor' },
    ],
  },
  {
    planet: 'Anvil (Fomal)',
    factions: [
      { id: 7, name: 'Ironclad', planet: 'Anvil' },
      { id: 8, name: 'Oreline', planet: 'Anvil' },
      { id: 9, name: 'Basalt', planet: 'Anvil' },
      { id: 10, name: 'Silicate', planet: 'Anvil' },
      { id: 11, name: 'Fission', planet: 'Anvil' },
    ],
  },
];

export const campaignFactionsFlat: CampaignFaction[] = campaignFactionsByPlanet.flatMap(
  (group) => group.factions,
);

export function campaignFactionLabel(id: number): string {
  const faction = campaignFactionsFlat.find((f) => f.id === id);
  return faction ? `${faction.name} (${id})` : `Faction ${id}`;
}
