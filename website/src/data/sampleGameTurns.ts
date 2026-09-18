/** SampleGame integration scenario — turns 1–6 (see Tests/SampleGame/SampleGame.cs). */

export type SampleGameTurn = {
  turn: number;
  story: string;
  reports: { faction: string; file: string }[];
  orders?: { faction: string; file: string }[];
};

export const sampleGameTurns: SampleGameTurn[] = [
  {
    turn: 1,
    story:
      'Sol: one planet, one moon, two surface regions. Caste Prime and Gelvaren start the quarter. ' +
      'Caste Prime builds a warship; Gelvaren raises a small army on Earth.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.1.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.1.3.txt' },
    ],
    orders: [
      { faction: 'Caste Prime (2)', file: 'orders.1.2.txt' },
      { faction: 'Gelvaren (3)', file: 'orders.1.3.txt' },
    ],
  },
  {
    turn: 2,
    story:
      'Caste Prime finishes a research lab and sends a hull toward Luna. Gelvaren assaults the rival region on Earth ' +
      'and completes the Sydney garrison contract, earning rocketry technology.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.2.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.2.3.txt' },
    ],
    orders: [
      { faction: 'Caste Prime (2)', file: 'orders.2.2.txt' },
      { faction: 'Gelvaren (3)', file: 'orders.2.3.txt' },
    ],
  },
  {
    turn: 3,
    story:
      'The lab begins research while the warship reaches Luna and discovers a crashed alien vessel. ' +
      'A UN contract offers the wreck to whoever completes the salvage research. Gelvaren secures Berlin; ' +
      'both factions publish press releases as the wreck quest opens.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.3.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.3.3.txt' },
    ],
    orders: [
      { faction: 'Caste Prime (2)', file: 'orders.3.2.txt' },
      { faction: 'Gelvaren (3)', file: 'orders.3.3.txt' },
    ],
  },
  {
    turn: 4,
    story:
      'Caste Prime wins the wreck: research points activate the alien hulk and copy fighter control tech into the library. ' +
      'Combat drones consolidate in the bay; a repair-tech breakthrough fires; an armor platoon leader finishes training. ' +
      'Gelvaren keeps building orbital industry.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.4.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.4.3.txt' },
    ],
    orders: [
      { faction: 'Caste Prime (2)', file: 'orders.4.2.txt' },
      { faction: 'Gelvaren (3)', file: 'orders.4.3.txt' },
    ],
  },
  {
    turn: 5,
    story:
      'The wreck stays on Luna while Caste Prime\'s warship returns to Earth orbit. Alien drones hangar-launch to meet ' +
      'Gelvaren\'s shuttle; Sydney factory nests an orbital rocket launcher aboard before the orbital battle.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.5.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.5.3.txt' },
    ],
    orders: [
      { faction: 'Caste Prime (2)', file: 'orders.5.2.txt' },
      { faction: 'Gelvaren (3)', file: 'orders.5.3.txt' },
    ],
  },
  {
    turn: 6,
    story:
      'End of the five-turn SampleGame arc: Caste Prime holds the Luna wreck and surviving drones in Earth orbit; ' +
      'Gelvaren\'s shuttle is gone after the orbital fight. These are the final golden reports — no further orders in the fixture.',
    reports: [
      { faction: 'Caste Prime (2)', file: 'testreport.6.2.txt' },
      { faction: 'Gelvaren (3)', file: 'testreport.6.3.txt' },
    ],
  },
];
