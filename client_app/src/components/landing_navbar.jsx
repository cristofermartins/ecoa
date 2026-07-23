import { useState, useEffect } from 'react';

export default function LandingNavbar() {
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 20);
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <nav
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        scrolled
          ? 'bg-white/92 backdrop-blur-md shadow-[0_2px_20px_rgba(0,0,0,0.08)] border-b border-[rgba(45,106,79,0.1)]'
          : 'bg-transparent border-b border-transparent'
      }`}
    >
      <div className="flex items-center justify-center px-6 py-4 max-w-[1400px] mx-auto w-full">
        <div className="flex flex-col items-center gap-1 no-underline">
          <span className="text-3xl leading-none">🌿</span>
          <span className="text-xl font-bold text-[#1b4332] transition-colors">
            Ecoa Santos
          </span>
        </div>
      </div>
    </nav>
  );
}
