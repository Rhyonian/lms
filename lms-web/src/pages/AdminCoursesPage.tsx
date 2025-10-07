import { useMemo } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { courses, type Course } from "@/data/courses";
import { DataTable } from "@/components/DataTable";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function AdminCoursesPage() {
  const columns = useMemo<ColumnDef<Course>[]>(
    () => [
      { accessorKey: "id", header: "ID" },
      { accessorKey: "title", header: "Course" },
      { accessorKey: "instructor", header: "Instructor" },
      { accessorKey: "category", header: "Category" },
      { accessorKey: "duration", header: "Duration" },
      {
        accessorKey: "level",
        header: "Level",
        cell: ({ getValue }) => <Badge variant="secondary">{String(getValue())}</Badge>
      }
    ],
    []
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Courses</CardTitle>
      </CardHeader>
      <CardContent>
        <DataTable data={courses} columns={columns} />
      </CardContent>
    </Card>
  );
}
