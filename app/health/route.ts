import { APP_NAME } from "@/lib/app-info";

// Evaluated on every request so it reflects the running process, and never cached.
export const dynamic = "force-dynamic";

export function GET() {
  return Response.json({ status: "ok", app: APP_NAME }, { headers: { "Cache-Control": "no-store" } });
}
