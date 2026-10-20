import Link from "next/link";
import { APP_NAME } from "@/lib/app-info";
import { NAV_ITEMS } from "@/lib/navigation";

export function SiteHeader() {
  return (
    <header className="border-b border-foreground/10">
      <div className="mx-auto flex w-full max-w-5xl items-center justify-between gap-4 px-4 py-3">
        <Link href="/" className="font-semibold">
          {APP_NAME}
        </Link>
        <nav aria-label="Main">
          <ul className="flex gap-4 text-sm">
            {NAV_ITEMS.map((item) => (
              <li key={item.href}>
                <Link href={item.href} className="hover:underline">
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
      </div>
    </header>
  );
}
