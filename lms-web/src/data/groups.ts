export type Group = {
  id: string;
  name: string;
  members: number;
  owner: string;
  status: "active" | "archived";
};

export const groups: Group[] = [
  { id: "GRP-001", name: "Product Onboarding", members: 32, owner: "Alex Johnson", status: "active" },
  { id: "GRP-002", name: "Sales Enablement", members: 18, owner: "Priya Desai", status: "active" },
  { id: "GRP-003", name: "Leadership Circle", members: 12, owner: "Jordan Smith", status: "archived" },
  { id: "GRP-004", name: "Engineering Guild", members: 45, owner: "Sofia Ivanova", status: "active" }
];
