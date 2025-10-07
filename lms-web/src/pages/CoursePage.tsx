import { Link, useParams } from "react-router-dom";
import { courses } from "@/data/courses";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function CoursePage() {
  const { courseId } = useParams();
  const course = courses.find((item) => item.id === courseId);

  if (!course) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-semibold">Course not found</h1>
        <Button asChild>
          <Link to="/catalog">Back to catalog</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="grid gap-6 lg:grid-cols-[2fr,1fr]">
        <Card className="overflow-hidden">
          <div className="aspect-video w-full bg-muted">
            <iframe
              title={course.title}
              src={course.heroVideoUrl}
              className="h-full w-full"
              allow="autoplay; fullscreen; picture-in-picture"
            />
          </div>
          <CardHeader>
            <CardTitle>{course.title}</CardTitle>
            <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
              <Badge variant="secondary">{course.category}</Badge>
              <span>{course.duration}</span>
              <span>{course.level}</span>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">{course.description}</p>
            <div className="text-sm">
              <span className="font-medium text-muted-foreground">Instructor:</span>{" "}
              <span className="font-semibold">{course.instructor}</span>
            </div>
            <Button asChild>
              <Link to={`/learn/${course.id}`}>Start learning</Link>
            </Button>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Lessons</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {course.lessons.map((lesson) => (
              <div key={lesson.id} className="space-y-1 border-b pb-3 last:border-b-0 last:pb-0">
                <div className="text-sm font-semibold">{lesson.title}</div>
                <Link
                  to={`/learn/${course.id}/${lesson.id}`}
                  className="text-sm text-primary hover:underline"
                >
                  Open lesson
                </Link>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
