import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function OidcCallbackPage() {
  const navigate = useNavigate();

  useEffect(() => {
    const timeout = setTimeout(() => navigate("/catalog"), 2500);
    return () => clearTimeout(timeout);
  }, [navigate]);

  return (
    <Card className="mx-auto max-w-xl">
      <CardHeader>
        <CardTitle>OIDC Callback Placeholder</CardTitle>
        <CardDescription>
          Simulates processing an identity token and redirecting learners back into the experience.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4 text-sm text-muted-foreground">
        <p>
          Parse the provider response, validate tokens, and persist session information here.
          Afterwards, route the learner to their destination.
        </p>
        <Button onClick={() => navigate("/catalog")}>Return to catalog</Button>
      </CardContent>
    </Card>
  );
}
