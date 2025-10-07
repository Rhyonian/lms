import { Link } from "react-router-dom";
import { courses } from "@/data/courses";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

export default function CatalogPage() {
  return (
    <div className="space-y-8">
      <header className="flex flex-col gap-2">
        <h1 className="text-3xl font-bold tracking-tight">Course Catalog</h1>
        <p className="text-muted-foreground">
          Browse curated learning experiences designed for modern enterprise teams.
        </p>
      </header>
      <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
        {courses.map((course) => (
          <Card key={course.id} className="flex flex-col">
            <CardHeader>
              <CardTitle>{course.title}</CardTitle>
              <CardDescription>{course.description}</CardDescription>
            </CardHeader>
            <CardContent className="flex flex-1 flex-col justify-between gap-4">
              <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
                <Badge variant="secondary">{course.category}</Badge>
                <span>{course.duration}</span>
                <span>{course.level}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="font-medium text-muted-foreground">Instructor</span>
                <span className="font-semibold">{course.instructor}</span>
              </div>
              <Link
                to={`/course/${course.id}`}
                className="text-sm font-semibold text-primary hover:underline"
              >
                View course
              </Link>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
