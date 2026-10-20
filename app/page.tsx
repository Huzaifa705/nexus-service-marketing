import { APP_DESCRIPTION, APP_NAME } from "@/lib/app-info";

const progress = [
  { node: "Node 1", what: "Requirements, decisions and architecture documents", state: "Complete" },
  { node: "Node 2", what: "Repository, tooling and baseline application (this build)", state: "In progress" },
  { node: "Node 3", what: "Database, sign-in and role permissions", state: "Next" },
];

export default function Home() {
  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-semibold tracking-tight">{APP_NAME}</h1>
      <p className="max-w-2xl text-lg text-foreground/80">{APP_DESCRIPTION}</p>
      <section aria-labelledby="progress-heading" className="space-y-2">
        <h2 id="progress-heading" className="text-xl font-semibold">
          Build progress
        </h2>
        <p className="text-foreground/80">No business features exist yet. This page shows the baseline only.</p>
        <ul className="divide-y divide-foreground/10 rounded border border-foreground/10">
          {progress.map((row) => (
            <li key={row.node} className="flex flex-wrap items-baseline justify-between gap-2 px-4 py-3">
              <span>
                <strong>{row.node}</strong> — {row.what}
              </span>
              <span className="text-sm text-foreground/70">{row.state}</span>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
