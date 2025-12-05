import Navbar from "./components/Navbar/Navbar";
import Hero from "./components/Hero/Hero";
import AboutSection from "./components/AboutSection/AboutSection";
import OpenSourceSection from "./components/OpenSourceSection/OpenSourceSection";
import ProductFeatures from "./components/ProductFeatures/ProductFeatures";
import Feature from "./components/Feature/Feature";
import HowToStartSection from "./components/HowToStartSection/HowToStartSection";
import CTASection from "./components/CTASection/CTASection";
import Footer from "./components/Footer/Footer";

import "./App.css";

function App() {
  return (
    <div className="App">
      <Navbar />
      <Hero />
      <AboutSection />
      <OpenSourceSection />
      <ProductFeatures />
      <Feature />
      <HowToStartSection />
      <CTASection />
      <Footer />
    </div>
  );
}

export default App;
