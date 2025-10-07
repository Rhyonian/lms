import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function OidcSignInPage() {
  return (
    <Card className="mx-auto max-w-xl">
      <CardHeader>
        <CardTitle>OIDC Sign-In Placeholder</CardTitle>
        <CardDescription>
          Connect this screen to your identity provider to start an interactive login flow.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4 text-sm text-muted-foreground">
        <p>
          This project scaffolds the UI only. Replace this text with the provider SDK logic for
          authentication, token retrieval, and session storage.
        </p>
        <Button className="w-full" disabled>
          Launch enterprise SSO
        </Button>
      </CardContent>
    </Card>
  );
}
