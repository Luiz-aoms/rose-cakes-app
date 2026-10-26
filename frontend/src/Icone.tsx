type Props = { indice: number }
const desenhos = [
  <><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></>,
  <><path d="m12 3 9 5-9 5-9-5 9-5Zm-9 5v9l9 5 9-5V8M12 13v9"/></>,
  <><circle cx="9" cy="8" r="3"/><path d="M3 21v-3a6 6 0 0 1 12 0v3M17 5a3 3 0 0 1 0 6M18 15a5 5 0 0 1 3 5"/></>,
  <><path d="M5 3h14v18l-3-2-4 2-4-2-3 2V3ZM8 8h8M8 12h8M8 16h4"/></>,
  <><path d="M4 5h16v14H4V5ZM4 9h16M8 14h3"/></>,
  <><path d="M4 3v18h17M8 17v-5M13 17V8M18 17V4"/></>,
  <><circle cx="12" cy="12" r="9"/><path d="M9 9a3 3 0 1 1 5 2c-2 1-2 2-2 3M12 17h.01"/></>,
]
export function Icone({ indice }: Props) {
  return <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{desenhos[indice] ?? desenhos[0]}</svg>
}
