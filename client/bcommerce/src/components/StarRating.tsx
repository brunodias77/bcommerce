import React, { useState } from "react";

interface StarRatingProps {
  totalStars?: number;
  onRate?: (rating: number) => void;
  className?: string; // Classe personalizada para o container
  starClassName?: string; // Classe personalizada para as estrelas
  size?: string; // Tamanho opcional das estrelas
}

const StarRating: React.FC<StarRatingProps> = ({ 
  totalStars = 5, 
  onRate, 
  className = "", 
  starClassName = "",
  size = "text-2xl" // Tamanho padrão das estrelas
}) => {
  const [rating, setRating] = useState<number>(0);
  const [hover, setHover] = useState<number>(0);

  const handleClick = (rate: number) => {
    setRating(rate);
    if (onRate) onRate(rate);
  };

  return (
    <div className={`flex space-x-1 cursor-pointer ${className}`}>
      {[...Array(totalStars)].map((_, index) => {
        const starValue = index + 1;
        return (
          <span
            key={index}
            className={`transition-colors duration-300 ${size} ${
              starValue <= (hover || rating) ? "text-yellow-400" : "text-gray-300"
            } ${starClassName}`}
            onClick={() => handleClick(starValue)}
            onMouseEnter={() => setHover(starValue)}
            onMouseLeave={() => setHover(0)}
          >
            ★
          </span>
        );
      })}
    </div>
  );
};

export default StarRating;
