export type User = {
  id: string;
  name: string;
  email: string;
  role: "learner" | "instructor" | "admin";
  status: "active" | "invited" | "suspended";
};

export const users: User[] = [
  {
    id: "USR-1001",
    name: "Alex Johnson",
    email: "alex.johnson@example.com",
    role: "admin",
    status: "active"
  },
  {
    id: "USR-1002",
    name: "Priya Desai",
    email: "priya.desai@example.com",
    role: "instructor",
    status: "active"
  },
  {
    id: "USR-1003",
    name: "Carlos Mendez",
    email: "carlos.mendez@example.com",
    role: "learner",
    status: "invited"
  },
  {
    id: "USR-1004",
    name: "Sofia Ivanova",
    email: "sofia.ivanova@example.com",
    role: "learner",
    status: "active"
  },
  {
    id: "USR-1005",
    name: "Jordan Smith",
    email: "jordan.smith@example.com",
    role: "instructor",
    status: "suspended"
  }
];
