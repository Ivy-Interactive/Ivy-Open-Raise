import React from "react";
import { Search } from "lucide-react";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";

interface HeroProps {
  title?: string;
  description?: string;
  smallTitle?: string;
  ctaText?: string;
  onCtaClick?: () => void;
}

const Hero: React.FC<HeroProps> = ({
  title = "Hero title",
  description = "Description",
  smallTitle = "Small title",
  ctaText = "Search",
  onCtaClick,
}) => {
  return (
    <div className="bg-primary-dark min-h-screen flex flex-col px-[200px] py-[100px] box-border xl:px-[100px] xl:py-[80px] md:px-6 md:py-[60px]">
      <div className="flex flex-col gap-[81px] w-full flex-1">
        <header className="flex items-center gap-2">
          <div className="flex items-center">
            <img
              src={logoSvg}
              alt="Logo"
              className="w-[153px] h-[74px]"
            />
            <span className="text-white font-inter text-[28px] font-medium tracking-tighter-2xl mx-2">
              by
            </span>
          </div>
          <span className="text-white font-inter text-[32px] font-bold tracking-tighter-lg">
            OpenRaise
          </span>
        </header>

        <div className="flex flex-col gap-6 flex-1">
          <h1 className="text-white font-inter text-[80px] font-bold tracking-tighter-3xl leading-[1.1] m-0 md:text-[48px] md:tracking-tighter-xl">
            {title}
          </h1>
          <p className="text-white font-inter text-2xl font-normal tracking-tighter leading-[1.5] m-0 md:text-lg md:tracking-tighter-md">
            {description}
          </p>

          <div className="flex flex-col gap-4 items-start">
            <p className="text-white font-inter text-sm font-normal tracking-tighter-sm m-0">
              {smallTitle}
            </p>
            <button
              className="bg-gray-light text-dark rounded-lg px-4 py-2 min-h-[36px] font-inter text-sm font-medium tracking-[0.07px] border-0 cursor-pointer inline-flex items-center justify-center gap-2 transition-all duration-200 ease-in-out hover:bg-gray-hover hover:-translate-y-[1px] active:translate-y-0"
              onClick={onCtaClick}
            >
              <Search className="w-4 h-4" />
              {ctaText}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Hero;