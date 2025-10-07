import { Link, useNavigate, useParams } from "react-router-dom";
import { courses } from "@/data/courses";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function LearnPage() {
  const { courseId, lessonId } = useParams();
  const navigate = useNavigate();

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

  const activeLesson = course.lessons.find((lesson) => lesson.id === lessonId) ?? course.lessons[0];
  const currentIndex = course.lessons.findIndex((lesson) => lesson.id === activeLesson.id);
  const previousLesson = course.lessons[currentIndex - 1];
  const nextLesson = course.lessons[currentIndex + 1];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <p className="text-sm text-muted-foreground">{course.title}</p>
          <h1 className="text-2xl font-semibold">{activeLesson.title}</h1>
        </div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <span>OIDC learner:</span>
          <span className="font-semibold">Pending integration</span>
        </div>
      </div>
      <Card className="overflow-hidden">
        <div className="aspect-video w-full bg-black/90">
          <iframe
            title={activeLesson.title}
            src={activeLesson.videoUrl}
            className="h-full w-full"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        </div>
        <CardContent className="space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <Button
              variant="outline"
              disabled={!previousLesson}
              onClick={() => previousLesson && navigate(`/learn/${course.id}/${previousLesson.id}`)}
            >
              Previous lesson
            </Button>
            <Button
              disabled={!nextLesson}
              onClick={() => nextLesson && navigate(`/learn/${course.id}/${nextLesson.id}`)}
            >
              Next lesson
            </Button>
          </div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Lesson playlist</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {course.lessons.map((lesson) => {
            const isActive = lesson.id === activeLesson.id;
            return (
              <button
                key={lesson.id}
                onClick={() => navigate(`/learn/${course.id}/${lesson.id}`)}
                className={`flex w-full items-center justify-between rounded-md border px-4 py-3 text-left text-sm transition-colors ${
                  isActive ? "border-primary bg-primary/10" : "hover:bg-muted"
                }`}
              >
                <span className="font-medium">{lesson.title}</span>
                {isActive && <span className="text-xs uppercase text-primary">Now playing</span>}
              </button>
            );
          })}
        </CardContent>
      </Card>
    </div>
  );
}
