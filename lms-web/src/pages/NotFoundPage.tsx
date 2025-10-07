import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export default function NotFoundPage() {
  return (
    <div className="flex flex-col items-start gap-4">
      <h1 className="text-3xl font-bold">Page not found</h1>
      <p className="text-muted-foreground">
        We couldn’t find the screen you were looking for. Explore the catalog to continue learning.
      </p>
      <Button asChild>
        <Link to="/catalog">Go to catalog</Link>
      </Button>
    </div>
  );
}
