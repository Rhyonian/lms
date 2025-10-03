import { useMemo, useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { users, type User } from "@/data/users";
import { DataTable } from "@/components/DataTable";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const statusVariant: Record<User["status"], "default" | "secondary" | "destructive"> = {
  active: "default",
  invited: "secondary",
  suspended: "destructive"
};

export default function AdminUsersPage() {
  const [search, setSearch] = useState("");
  const filteredUsers = useMemo(() => {
    return users.filter((user) => {
      const term = search.toLowerCase();
      return (
        user.name.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term) ||
        user.role.toLowerCase().includes(term)
      );
    });
  }, [search]);

  const columns = useMemo<ColumnDef<User>[]>(
    () => [
      { accessorKey: "id", header: "ID" },
      { accessorKey: "name", header: "Name" },
      { accessorKey: "email", header: "Email" },
      {
        accessorKey: "role",
        header: "Role",
        cell: ({ getValue }) => (
          <span className="capitalize text-sm font-medium">{String(getValue())}</span>
        )
      },
      {
        accessorKey: "status",
        header: "Status",
        cell: ({ getValue }) => {
          const value = getValue() as User["status"];
          return <Badge variant={statusVariant[value]}>{value}</Badge>;
        }
      }
    ],
    []
  );

  return (
    <Card>
      <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <CardTitle>Users</CardTitle>
        <Input
          placeholder="Search by name, email, or role"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="max-w-sm"
        />
      </CardHeader>
      <CardContent>
        <DataTable data={filteredUsers} columns={columns} />
      </CardContent>
    </Card>
  );
}
