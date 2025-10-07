import { useMemo } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { groups, type Group } from "@/data/groups";
import { DataTable } from "@/components/DataTable";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const statusVariant: Record<Group["status"], "default" | "secondary"> = {
  active: "default",
  archived: "secondary"
};

export default function AdminGroupsPage() {
  const columns = useMemo<ColumnDef<Group>[]>(
    () => [
      { accessorKey: "id", header: "ID" },
      { accessorKey: "name", header: "Group" },
      { accessorKey: "owner", header: "Owner" },
      {
        accessorKey: "members",
        header: "Members",
        cell: ({ getValue }) => <span className="font-medium">{getValue<number>()}</span>
      },
      {
        accessorKey: "status",
        header: "Status",
        cell: ({ getValue }) => {
          const value = getValue() as Group["status"];
          return <Badge variant={statusVariant[value]}>{value}</Badge>;
        }
      }
    ],
    []
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Groups</CardTitle>
      </CardHeader>
      <CardContent>
        <DataTable data={groups} columns={columns} />
      </CardContent>
    </Card>
  );
}
